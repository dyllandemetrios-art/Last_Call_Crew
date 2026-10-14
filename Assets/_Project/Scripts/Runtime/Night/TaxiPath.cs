using UnityEngine;

namespace LastCallCrew.Night
{
    /// <summary>
    /// Trajet du taxi (P1-055), pur : polyligne de la route réservée aux virages adoucis, parcourue de A vers B ou de B vers A.
    /// Chaque instance l'échantillonne localement à partir des mêmes temps serveur ; aucune position n'est répliquée.
    /// Axe de la carte : A (0 ; 8) → (0 ; −4) → (−130 ; −4) → B (−130 ; 13).
    /// </summary>
    public sealed class TaxiPath
    {
        public static readonly Vector3[] DefaultAxis =
        {
            new Vector3(0f, 0f, 8f),
            new Vector3(0f, 0f, -4f),
            new Vector3(-130f, 0f, -4f),
            new Vector3(-130f, 0f, 13f),
        };

        readonly Vector3[] _points;
        readonly float[] _cumulative;
        readonly float _stopYawA;
        readonly float _stopYawB;
        readonly float _turnBlendMeters;

        public float Length { get; }

        /// <param name="axis">Axe de A vers B (au moins deux points).</param>
        /// <param name="cornerRadius">Rayon (m) du raccord adouci de chaque virage.</param>
        /// <param name="stopYawA">Cap (degrés) du taxi garé à A ; idem B. Le cap se fond dans la tangente sur <paramref name="turnBlendMeters"/>.</param>
        public TaxiPath(Vector3[] axis, float cornerRadius, float stopYawA, float stopYawB, float turnBlendMeters = 14f)
        {
            if (axis == null || axis.Length < 2)
                throw new System.ArgumentException("Au moins deux points d'axe sont requis.", nameof(axis));
            _stopYawA = stopYawA;
            _stopYawB = stopYawB;
            _turnBlendMeters = Mathf.Max(0.01f, turnBlendMeters);
            _points = Smooth(axis, Mathf.Max(0f, cornerRadius));
            _cumulative = new float[_points.Length];
            for (int i = 1; i < _points.Length; i++)
                _cumulative[i] = _cumulative[i - 1] + Vector3.Distance(_points[i - 1], _points[i]);
            Length = _cumulative[_cumulative.Length - 1];
        }

        public static TaxiPath CreateDefault() => new TaxiPath(DefaultAxis, 8f, 180f, 180f);

        static Vector3[] Smooth(Vector3[] axis, float radius)
        {
            var list = new System.Collections.Generic.List<Vector3> { axis[0] };
            for (int i = 1; i < axis.Length - 1; i++)
            {
                Vector3 prev = axis[i - 1], corner = axis[i], next = axis[i + 1];
                float r = Mathf.Min(radius, Vector3.Distance(prev, corner) * 0.5f, Vector3.Distance(corner, next) * 0.5f);
                Vector3 a = corner + (prev - corner).normalized * r;
                Vector3 b = corner + (next - corner).normalized * r;
                const int steps = 10;
                for (int s = 0; s <= steps; s++)
                {
                    float t = s / (float)steps;
                    list.Add(Vector3.Lerp(Vector3.Lerp(a, corner, t), Vector3.Lerp(corner, b, t), t)); // Bézier quadratique
                }
            }
            list.Add(axis[axis.Length - 1]);
            return list.ToArray();
        }

        /// <summary>Progression lissée 0..1 d'un trajet de durée normalisée t (accélération et freinage doux).</summary>
        public static float Ease(float t)
        {
            t = Mathf.Clamp01(t);
            return Mathf.Lerp(t, t * t * (3f - 2f * t), 0.7f);
        }

        /// <summary>Position et cap du taxi après la fraction <paramref name="t"/> (0..1) du trajet. <paramref name="towardsB"/> : sens A → B.</summary>
        public void Sample(float t, bool towardsB, out Vector3 position, out float yawDegrees)
        {
            float travelled = Ease(t) * Length;
            float along = towardsB ? travelled : Length - travelled;
            position = PositionAt(along, out Vector3 tangent);
            if (!towardsB)
                tangent = -tangent;
            float tangentYaw = Mathf.Atan2(tangent.x, tangent.z) * Mathf.Rad2Deg;
            float startYaw = towardsB ? _stopYawA : _stopYawB;
            float endYaw = towardsB ? _stopYawB : _stopYawA;
            float yaw = tangentYaw;
            if (travelled < _turnBlendMeters)
                yaw = Mathf.LerpAngle(startYaw, tangentYaw, Smooth01(travelled / _turnBlendMeters));
            float remaining = Length - travelled;
            if (remaining < _turnBlendMeters)
                yaw = Mathf.LerpAngle(endYaw, yaw, Smooth01(remaining / _turnBlendMeters));
            yawDegrees = yaw;
        }

        static float Smooth01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        Vector3 PositionAt(float distance, out Vector3 tangent)
        {
            distance = Mathf.Clamp(distance, 0f, Length);
            int i = 1;
            while (i < _cumulative.Length - 1 && _cumulative[i] < distance)
                i++;
            float segment = _cumulative[i] - _cumulative[i - 1];
            float f = segment > 1e-5f ? (distance - _cumulative[i - 1]) / segment : 0f;
            tangent = (_points[i] - _points[i - 1]).normalized;
            return Vector3.Lerp(_points[i - 1], _points[i], f);
        }
    }
}
