using UnityEngine;

namespace Combat
{
    public sealed class DistanceBandEvaluator
    {
        private readonly float _meleeSqr;
        private readonly float _closeSqr;
        private readonly float _midSqr;

        public DistanceBandEvaluator(float melee, float close, float mid)
        {
            _meleeSqr = melee * melee;
            _closeSqr = close * close;
            _midSqr   = mid   * mid;
        }

        public DistanceBand Evaluate(Vector3 origin, Vector3 target)
        {
            float sqrDist = (target - origin).sqrMagnitude;

            if (sqrDist <= _meleeSqr) return DistanceBand.Melee;
            if (sqrDist <= _closeSqr) return DistanceBand.Close;
            if (sqrDist <= _midSqr)   return DistanceBand.Mid;
            return DistanceBand.Far;
        }
    }
}