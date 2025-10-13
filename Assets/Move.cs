using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Move : Command
{
    public Vector3 movePosition;
    public Move(Entity ent, Vector3 pos) : base(ent)
    {
        movePosition = pos;
    }

    public LineRenderer potentialLine;
    public override void Init()
    {
        //Debug.Log("MoveInit:\tMoving to: " + movePosition);
        line = LineMgr.inst.CreateMoveLine(entity.position, movePosition);

        if (AIMgr.inst.isPotentialFieldsMovement)
        {
            potentialLine = LineMgr.inst.CreatePotentialLine(entity.position);
            line.gameObject.SetActive(true);
        }
    }

    public override void Tick()
    {
        DHDS dhds;
        if (AIMgr.inst.isPotentialFieldsMovement)
            dhds = ComputePotentialDHDS();
        else
            dhds = ComputeDHDS();

        entity.desiredHeading = dhds.dh;
        entity.desiredSpeed = dhds.ds;
        line.SetPosition(1, movePosition);
    }

    public Vector3 diff = Vector3.positiveInfinity;
    public float dhRadians;
    public float dhDegrees;
    public DHDS ComputeDHDS()
    {
        diff = movePosition - entity.position;
        dhRadians = Mathf.Atan2(diff.x, diff.z);
        dhDegrees = Utils.Degrees360(Mathf.Rad2Deg * dhRadians);
        return new DHDS(dhDegrees, entity.maxSpeed);

    }

    public DHDS ComputePotentialDHDS()
    {
        Potential p;
        repulsivePotential = Vector3.one; repulsivePotential.y = 0;
        foreach (Entity ent in EntityMgr.inst.entities)
        {
            if (ent == entity) continue;
            p = DistanceMgr.inst.GetPotential(entity, ent);
            if (p.distance < AIMgr.inst.potentialDistanceThreshold)
            {
                repulsivePotential += Vector3.Min(AIMgr.inst.repulsiveCoefficient * entity.mass * Mathf.Pow(p.diff.magnitude, AIMgr.inst.repulsiveExponent) * p.direction, new Vector3(1000, 1000, 100));
                //repulsivePotential += p.diff;
            }
        }

        Vector3 maxPotential = new Vector3(1000, 1000, 1000);

        foreach (GameObject obstacle in EnvironmentMgr.inst.circlePool)
        {
            if (!obstacle.activeInHierarchy)
                continue;

            float distance;
            Vector3 closestPoint = obstacle.GetComponent<SphereCollider>().ClosestPointOnBounds(entity.position);
            Vector3 displacement = closestPoint - entity.position;
            distance = displacement.magnitude;
            Vector3 direction = displacement.normalized;

            if (distance < AIMgr.inst.potentialDistanceThreshold)
            {
                repulsivePotential += Vector3.Min(
                    AIMgr.inst.repulsiveCoefficient
                    * entity.mass
                    * Mathf.Pow(distance, AIMgr.inst.repulsiveExponent)
                    * direction, maxPotential);
            }
        }
        foreach (GameObject obstacle in EnvironmentMgr.inst.rectanglePool)
        {
            if (!obstacle.activeInHierarchy)
                continue;

            float distance;
            Vector3 closestPoint = obstacle.GetComponent<BoxCollider>().ClosestPointOnBounds(entity.position);
            Vector3 displacement = closestPoint - entity.position;
            distance = displacement.magnitude;
            Vector3 direction = displacement.normalized;
            if (distance < AIMgr.inst.potentialDistanceThreshold)
            {
                repulsivePotential += Vector3.Min(
                    AIMgr.inst.repulsiveCoefficient
                    * entity.mass
                    * Mathf.Pow(distance, AIMgr.inst.repulsiveExponent)
                    * direction, maxPotential);
            }
        }
        // repulsivePotential *= repulsiveCoefficient * Mathf.Pow(repulsivePotential.magnitude, repulsiveExponent);
        attractivePotential = movePosition - entity.position;
        Vector3 tmp = attractivePotential.normalized;
        attractivePotential = AIMgr.inst.attractionCoefficient * Mathf.Pow(attractivePotential.magnitude, AIMgr.inst.attractiveExponent) * tmp;
        potentialSum = attractivePotential - repulsivePotential;

        dh = Utils.Degrees360(Mathf.Rad2Deg * Mathf.Atan2(potentialSum.x, potentialSum.z));

        angleDiff = Utils.Degrees360(Utils.AngleDiffPosNeg(dh, entity.heading));
        cosValue = (Mathf.Cos(angleDiff * Mathf.Deg2Rad) + 1) / 2.0f; // makes it between 0 and 1
        ds = entity.maxSpeed * cosValue;

        return new DHDS(dh, ds);
    }
    public Vector3 attractivePotential = Vector3.zero;
    public Vector3 potentialSum = Vector3.zero;
    public Vector3 repulsivePotential = Vector3.zero;
    public float dh;
    public float angleDiff;
    public float cosValue;
    public float ds;



    public float doneDistanceSq = 1000;
    public override bool IsDone()
    {

        return ((entity.position - movePosition).sqrMagnitude < doneDistanceSq);
    }

    public override void Stop()
    {
        entity.desiredSpeed = 0;
        LineMgr.inst.DestroyLR(line);
        LineMgr.inst.Destroy(potentialLine);
    }
}
