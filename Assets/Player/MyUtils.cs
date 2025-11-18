using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;


public delegate void DoSomeWithTransform(Transform transform);
public static class MyUtils
{
    //Every shader graph tutorial
    //https://danielilett.com/2021-05-20-every-shader-graph-node/

    public static float radiansToDegrees = 57.295779513082320876798154814105F;
    public static float degreesToRadians = 0.01745329251994329576923690768489F;
    public static Transform freeUsableTransform = new GameObject().transform;
    public static System.Random rand = new System.Random();

    public static Vector2 rotateVector(Vector2 vector, float angle)
    {
        angle = angle * degreesToRadians;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        float rotX = vector.x * cos - vector.y * sin;
        float rotY = vector.x * sin + vector.y * cos;
        return new Vector2(rotX, rotY);
    }

    public static int next(int value, int amountnext, int bound)
    {
        int result = value + amountnext;
        if (result >= bound && amountnext < bound)
        {
            result = result - bound;
        }
        if (amountnext == bound)
        {
            return value;
        }
        if (amountnext > bound)
        {
            int perc = amountnext % bound;
            result = value + perc;
        }
        return result;
    }

    public static Collider[] GetAllCollidersTouchingBox(GameObject box)
    {

        Vector3 half = new Vector3(box.transform.localScale.x / 2, box.transform.localScale.y / 2, box.transform.localScale.z / 2);
        return Physics.OverlapBox(box.transform.position, half, box.transform.rotation);
    }

    public static Transform GetHighestParent(Transform parentOf)
    {
        for (int i = 0; i < 100; i++)
        {
            if (parentOf.parent != null)
            {
                parentOf = parentOf.parent;
            }
            else
            {
                if (i == 0)
                    return null;
                return parentOf;
            }
        }
        return parentOf;
    }

    public static void CopyTransformValues(Transform from, Transform to)
    {
        to.position = from.position;
        to.rotation = from.rotation;
        to.localScale = from.localScale;
    }

    public static Transform FindChildInHierarchy(Transform transformIn, string nameToSearch)
    {
        for (int i = 0; i < transformIn.childCount; i++)
        {
            Transform child = transformIn.GetChild(i);
            if (child.name == nameToSearch)
                return child;
            else if (child.childCount > 0)
            {
                Transform finded = FindChildInHierarchy(child, nameToSearch);
                if (finded != null)
                    return finded;
            }
        }
        return null;
    }

    public static void ToAllChildsInHierarchy(Transform transformIn, DoSomeWithTransform doSomeWithTransform)
    {
        if (transformIn.childCount > 0)
            for (int i = 0; i < transformIn.childCount; i++)
            {
                Transform child = transformIn.GetChild(i);
                doSomeWithTransform.Invoke(child);
                ToAllChildsInHierarchy(child, doSomeWithTransform);
            }
    }

    public static void DestroyAllColliders(GameObject gameObject)
    {
        Collider[] colliders = gameObject.GetComponents<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            GameObject.Destroy(colliders[i]);
        }
    }

    public static float Distance(this Vector3 pos, Vector3 distanceTo)
    {
        float xx = pos.x - distanceTo.x;
        float yy = pos.y - distanceTo.y;
        float zz = pos.z - distanceTo.z;
        return (float)Math.Sqrt(xx * xx + yy * yy + zz * zz);
    }
    public static float DistanceSq(this Vector3 pos, Vector3 distanceTo)
    {
        float xx = pos.x - distanceTo.x;
        float yy = pos.y - distanceTo.y;
        float zz = pos.z - distanceTo.z;
        return xx * xx + yy * yy + zz * zz;
    }

    public static float DistanceXZ(this Vector3 pos, Vector3 distanceTo)
    {
        float xx = pos.x - distanceTo.x;
        float zz = pos.z - distanceTo.z;
        return (float)Math.Sqrt(xx * xx + zz * zz);
    }
    public static float DistanceXZSq(this Vector3 pos, Vector3 distanceTo)
    {
        float xx = pos.x - distanceTo.x;
        float zz = pos.z - distanceTo.z;
        return xx * xx + zz * zz;
    }

    public static float AddMoveTowards(GameObject gameObject, float power, Vector3 posTo, bool useY)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            return AddMoveTowards(rigidbody, power, posTo, useY);
        }
        return useY ? gameObject.transform.position.Distance(posTo) : gameObject.transform.position.DistanceXZ(posTo);
    }

    public static float AddMoveTowards(Rigidbody rb, float power, Vector3 posTo, bool useY)
    {
        if (useY)
        {
            float dist = rb.position.Distance(posTo);
            if (dist > 0.0001F)
            {
                float prunex = (float)((posTo.x - rb.position.x) / dist * power);
                float pruney = (float)((posTo.y - rb.position.y) / dist * power);
                float prunez = (float)((posTo.z - rb.position.z) / dist * power);

                rb.linearVelocity = new Vector3(rb.linearVelocity.x + prunex, rb.linearVelocity.y + pruney, rb.linearVelocity.z + prunez);
            }
            return dist;
        }
        else
        {
            float dist = rb.position.DistanceXZ(posTo);
            if (dist > 0.0001F)
            {

                float prunex = (float)((posTo.x - rb.position.x) / dist * power);
                float prunez = (float)((posTo.z - rb.position.z) / dist * power);

                rb.linearVelocity = new Vector3(rb.linearVelocity.x + prunex, rb.linearVelocity.y, rb.linearVelocity.z + prunez);
            }
            return dist;
        }
    }

    public static float AddMoveTowards(Rigidbody rb, float powerXZ, float powerY, Vector3 posTo)
    {
        if (powerY != 0)
        {
            float dist = rb.position.Distance(posTo);
            if (dist > 0.0001F)
            {
                float prunex = (float)((posTo.x - rb.position.x) / dist * powerXZ);
                float pruney = (float)((posTo.y - rb.position.y) / dist * powerY);
                float prunez = (float)((posTo.z - rb.position.z) / dist * powerXZ);

                rb.linearVelocity = new Vector3(rb.linearVelocity.x + prunex, rb.linearVelocity.y + pruney, rb.linearVelocity.z + prunez);
            }
            return dist;
        }
        else
        {
            float dist = rb.position.DistanceXZ(posTo);
            if (dist > 0.0001F)
            {

                float prunex = (float)((posTo.x - rb.position.x) / dist * powerXZ);
                float prunez = (float)((posTo.z - rb.position.z) / dist * powerXZ);

                rb.linearVelocity = new Vector3(rb.linearVelocity.x + prunex, rb.linearVelocity.y, rb.linearVelocity.z + prunez);
            }
            return dist;
        }
    }

    public static void SetMoveTowards(Rigidbody rb, float power, Vector3 posTo, bool useY)
    {
        if (useY)
        {
            float dist = rb.position.Distance(posTo);

            float prunex = (float)((posTo.x - rb.position.x) / dist * power);
            float pruney = (float)((posTo.y - rb.position.y) / dist * power);
            float prunez = (float)((posTo.z - rb.position.z) / dist * power);

            rb.linearVelocity = new Vector3(prunex, pruney, prunez);
        }
        else
        {
            float dist = rb.position.DistanceXZ(posTo);

            float prunex = (float)((posTo.x - rb.position.x) / dist * power);
            float prunez = (float)((posTo.z - rb.position.z) / dist * power);

            rb.linearVelocity = new Vector3(prunex, rb.linearVelocity.y, prunez);
        }
    }

    public static void Friction(Rigidbody rb, float multiplier, bool useY)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x * multiplier, useY ? (rb.linearVelocity.y * multiplier) : (rb.linearVelocity.y), rb.linearVelocity.z * multiplier);
    }
    //public static void Friction(Rigidbody rb, float multiplierXZ, float multiplierY)
    //{
    //    rb.velocity = new Vector3(rb.velocity.x * multiplierXZ, rb.velocity.y * multiplierY, rb.velocity.z * multiplierXZ);
    //}

    public static void AddMove(GameObject gameObject, float x, float y, float z)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x + x, rigidbody.linearVelocity.y + y, rigidbody.linearVelocity.z + z);
        }
    }

    public static void SetMove(GameObject gameObject, float x, float y, float z)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = new Vector3(x, y, z);
        }
    }

    public static void AddMove(GameObject gameObject, Vector3 move)
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = new Vector3(rigidbody.linearVelocity.x + move.x, rigidbody.linearVelocity.y + move.y, rigidbody.linearVelocity.z + move.z);
        }
    }

    // pitch = transform.eulerAngles.x    yaw = transform.eulerAngles.y

    public static Vector3 PitchYawToVector3(float pitch, float yaw)
    {
        float f = (float)Math.Cos(-yaw * 0.017453292F - Math.PI);
        float f1 = (float)Math.Sin(-yaw * 0.017453292F - Math.PI);
        float f2 = (float)-Math.Cos(-pitch * 0.017453292F);
        float f3 = (float)Math.Sin(-pitch * 0.017453292F);
        return new Vector3((f1 * f2), f3, (f * f2));
    }

    public static Vector3 YawToVector3(float yaw)
    {
        float f = (float)Math.Cos(-yaw * 0.017453292F - (float)Math.PI);
        float f1 = (float)Math.Sin(-yaw * 0.017453292F - (float)Math.PI);
        return new Vector3((-f1), 0, (-f));
    }

    public static Vector2 YawToVector2(float yaw)
    {
        float f = (float)Math.Cos(-yaw * 0.017453292F - (float)Math.PI);
        float f1 = (float)Math.Sin(-yaw * 0.017453292F - (float)Math.PI);
        return new Vector2((-f1), (-f));
    }

    public static Vector2 Vector3ToPitchYaw(Vector3 vec)
    {


        float f = (float)Math.Sqrt(vec.x * vec.x + vec.z * vec.z);
        float rotationYaw = (float)((float)Math.Atan2(vec.x, -vec.z) * (180D / Math.PI));
        float rotationPitch = (float)((float)Math.Atan2(vec.y, (double)f) * (180D / Math.PI));

        return new Vector2(rotationPitch, rotationYaw);

    }

    public static float Vector2ToYaw(float x, float z)
    {
        float f = (float)Math.Sqrt(x + z);
        float rotationYaw = (float)((float)Math.Atan2(x, -z) * (180D / Math.PI));
        return rotationYaw;
    }

    /**
	 * ���������� ������������ (�� ������) ���������� ���� ����� ���� ������. ���� � ��������
	 * 
	 */
    public static float GetDirectionBetweenAngles(float baseValue, float targetValue)
    {
        baseValue = WrapDegrees(baseValue) + 180;
        targetValue = WrapDegrees(targetValue) + 180;
        if (targetValue == baseValue)
        {
            //System.out.println("angles0:     baseValue: " + baseValue + "   targetValue: " + targetValue + "   result: " + (0));
            return 0;
        }
        float baseValue180 = WrapDegrees(baseValue) + 180;
        if (targetValue == baseValue180)
        {
            //System.out.println("angles180:     baseValue: " + baseValue + "   value180: " + baseValue180 + "   targetValue: " + targetValue + "   result: " + (180));
            return 180;
        }

        if (baseValue180 > baseValue)
        {
            if (targetValue > baseValue && targetValue < baseValue180)
            {
                //System.out.println("angles1:     baseValue: " + baseValue + "   value180: " + baseValue180  + "   targetValue: " + targetValue + "   result: " + (targetValue - baseValue));
                return targetValue - baseValue;
            }
            else
            {
                if (targetValue >= 0 && targetValue < baseValue)
                {
                    //System.out.println("angles2:     baseValue: " + baseValue + "   value180: " + baseValue180  + "   targetValue: " + targetValue + "   result: " + (targetValue - baseValue));
                    return targetValue - baseValue;
                }
                if (targetValue <= 360 && targetValue > baseValue180)
                {
                    //System.out.println("angles3:     baseValue: " + baseValue + "   value180: " + baseValue180  + "   targetValue: " + targetValue + "   result: " + (-baseValue - (360 - targetValue)));
                    return -baseValue - (360 - targetValue);
                }
            }
        }
        else
        {
            if (targetValue < baseValue && targetValue > baseValue180)
            {
                //System.out.println("angles4:     baseValue: " + baseValue + "   value180: " + baseValue180  + "   targetValue: " + targetValue + "   result: " + (targetValue - baseValue));
                return targetValue - baseValue;
            }
            else
            {
                if (targetValue >= 0 && targetValue < baseValue180)
                {
                    //System.out.println("angles5:     baseValue: " + baseValue + "   value180: " + baseValue180  + "   targetValue: " + targetValue + "   result: " + ((360 - baseValue) + targetValue));
                    return (360 - baseValue) + targetValue;
                }
                if (targetValue <= 360 && targetValue > baseValue)
                {
                    //System.out.println("angles6:     baseValue: " + baseValue + "   value180: " + baseValue180  + "   targetValue: " + targetValue + "   result: " + (targetValue - baseValue));
                    return targetValue - baseValue;
                }
            }
        }
        //System.out.println("angles7:     baseValue: " + baseValue + "   value180: " + baseValue180  + "   targetValue: " + targetValue + "   result: " + (0));
        return 0;
    }

    public static float WrapDegrees(float value)
    {
        value = value % 360.0F;

        if (value >= 180.0F)
        {
            value -= 360.0F;
        }

        if (value < -180.0F)
        {
            value += 360.0F;
        }

        return value;
    }

    /// <summary>
    /// Directions must be normalized!
    /// </summary>
    public static Vector3[] GetTentaclePositions(Vector3 positionStart, Vector3 directionStart, Vector3 positionEnd, Vector3 directionEnd, float directionsLength, int posesAmount)
    {
        Vector3[] poses = new Vector3[posesAmount];
        float oneFragm = directionsLength / (float)(posesAmount - 1);
        for (int i = 0; i < posesAmount; i++)
        {
            float ratio = Softfromto(i, 0, posesAmount - 1);
            float unratio = 1F - ratio;
            float lengthSplitted = oneFragm * i;
            float unlengthSplitted = directionsLength - lengthSplitted;

            Vector3 line1 = new Vector3(
                positionStart.x + directionStart.x * lengthSplitted,
                positionStart.y + directionStart.y * lengthSplitted,
                positionStart.z + directionStart.z * lengthSplitted);

            Vector3 line2 = new Vector3(
                positionEnd.x + directionEnd.x * unlengthSplitted,
                positionEnd.y + directionEnd.y * unlengthSplitted,
                positionEnd.z + directionEnd.z * unlengthSplitted);

            poses[i] = new Vector3(line1.x * unratio + line2.x * ratio, line1.y * unratio + line2.y * ratio, line1.z * unratio + line2.z * ratio);



        }
        return poses;
    }

    public static Vector3 GetPositionOnTentacle(Vector3 positionStart, Vector3 directionStart, Vector3 positionEnd, Vector3 directionEnd, float directionsLength, float progress_from0to1)
    {
        float oneFragm = directionsLength;

        float ratio = progress_from0to1;//Softfromto(i, 0, posesAmount - 1);
        float unratio = 1F - ratio;
        float lengthSplitted = oneFragm * progress_from0to1;
        float unlengthSplitted = directionsLength - lengthSplitted;

        Vector3 line1 = new Vector3(
            positionStart.x + directionStart.x * lengthSplitted,
            positionStart.y + directionStart.y * lengthSplitted,
            positionStart.z + directionStart.z * lengthSplitted);

        Vector3 line2 = new Vector3(
            positionEnd.x + directionEnd.x * unlengthSplitted,
            positionEnd.y + directionEnd.y * unlengthSplitted,
            positionEnd.z + directionEnd.z * unlengthSplitted);

        return new Vector3(line1.x * unratio + line2.x * ratio, line1.y * unratio + line2.y * ratio, line1.z * unratio + line2.z * ratio);



    }

    public static Vector3 MixVectors(Vector3 vector1, Vector3 vector2, float vector1_ratio)
    {
        float unratio = 1F - vector1_ratio;
        return new Vector3(
            vector1.x * vector1_ratio + vector2.x * unratio,
            vector1.y * vector1_ratio + vector2.y * unratio,
            vector1.z * vector1_ratio + vector2.z * unratio
            );
    }

    public static Vector3 RandomVector(float range)
    {
        if (range == 0)
            return Vector3.zero;
        return new Vector3(UnityEngine.Random.Range(-range, range), UnityEngine.Random.Range(-range, range), UnityEngine.Random.Range(-range, range));
    }

    public static float Getfromto(float mainNumber, float from, float to)//�� ��������. ���������� ����� �� 0 �� 1, ��������������� ��������� �� �������, �� ����� mainNumber (�� from �� to),  ..�� ��� ��� ���������
    {
        if (mainNumber < from)
        {
            return 0;
        }
        else if (mainNumber > to)
        {
            return 1;
        }
        else
        {
            float n = Math.Max(mainNumber - from, 0);
            float f = 1F / (to - from);
            return n * f;
        }

    }

    public static float Softfromto(float mainNumber, float from, float to)//�� ��������. ���������� ����� �� 0 �� 1, ��������������� ��������� �� �������, �� ����� mainNumber (�� from �� to) �� ������������
    {
        if (mainNumber < from)
        {
            return 0;
        }
        else if (mainNumber > to)
        {
            return 1;
        }
        else
        {
            return ((float)-Math.Cos((mainNumber - from) * ((float)Math.PI) / (to - from))) / 2F + 0.5F;
        }

    }

    /// <summary>
    /// Destroys CharacterJoints, Rigidbodies, Colliders if has
    /// </summary>
    public static void DestroyAllPhysicsInHierarchy(Transform transform)
    {
        DoSomeWithTransform doSome = (Transform child) =>
        {
            CharacterJoint joint = child.GetComponent<CharacterJoint>();
            if (joint != null)
            {
                GameObject.Destroy(joint);
            }

            Rigidbody rigidbody = child.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                GameObject.Destroy(rigidbody);
            }

            DestroyAllColliders(child.gameObject);
        };

        ToAllChildsInHierarchy(transform, doSome);
        doSome.Invoke(transform);
    }

    public static void SetPositionWithParentsAndChilds(Transform transform, Vector3 pos)
    {
        Transform parent = GetHighestParent(transform);
        Vector3 vecDifference = pos - transform.position;
        parent.position = parent.position + vecDifference;

        //float x;
        //float y;
        //float z;
        //for (int i = 0; i < 100; i++)
        //{
        //    if (transform.parent != null)
        //    {
        //        parentOf = parentOf.parent;
        //    }
        //    else
        //    {
        //        if (i == 0)
        //            return null;
        //        return parentOf;
        //    }
        //}
        //return parentOf;
    }

    public static Color DecimaltoRGB(int color)
    {
        float R = (float)(color >> 16 & 255) / 255.0F;
        float G = (float)(color >> 8 & 255) / 255.0F;
        float B = (float)(color & 255) / 255.0F;
        return new Color(R, G, B);
    }

    public static T GetNearest<T>(IEnumerable<T> elements, Vector3 positionTo, float findDistance, FindCondition<T> findCondition = null) where T : MonoBehaviour
    {
        float minDist = findDistance * findDistance;
        T ret = null;
        foreach (T element in elements)
        {
            float dist = positionTo.DistanceSq(element.transform.position);
            if (dist < minDist && (findCondition == null || findCondition.Invoke(element)))
            {
                minDist = dist;
                ret = element;
            }
        }
        return ret;
    }

    public delegate bool FindCondition<T>(T element) where T : MonoBehaviour;

    public static float FollowNumber(float baseValue, float targetValue, float followAmount)
    {
        if (baseValue < targetValue)
        {
            return Math.Min(baseValue + followAmount, targetValue);
        }
        if (baseValue > targetValue)
        {
            return Math.Max(baseValue - followAmount, targetValue);
        }
        return baseValue;
    }

    public static Vector3 Follow(Vector3 baseValue, Vector3 targetValue, float followAmount)
    {
        Vector3 difference = baseValue - targetValue;
        float distance = difference.magnitude;
        if (distance <= 0)
            return targetValue;
        float newdistance = Math.Max(distance - followAmount, 0);
        float mult = newdistance / distance;
        return new Vector3(targetValue.x + difference.x * mult, targetValue.y + difference.y * mult, targetValue.z + difference.z * mult);

    }

    public static Color FollowColor(Color baseValue, Color targetValue, float followAmount)
    {
        Color newCol = new Color(
            baseValue.r + (targetValue.r - baseValue.r) * followAmount,
            baseValue.g + (targetValue.g - baseValue.g) * followAmount,
            baseValue.b + (targetValue.b - baseValue.b) * followAmount,
            baseValue.a + (targetValue.a - baseValue.a) * followAmount
            );
        return newCol;
    }

    public static Color GetRainbow(float hue, float alpha)
    {
        float f1 = 1F / 6F;
        float f2 = f1 * 2;
        float f3 = f1 * 3;
        float f4 = f1 * 4;
        float f5 = f1 * 5;

        float red = 1F - Getfromto(hue, f1, f2) + Getfromto(hue, f4, f5);
        float green = Getfromto(hue, 0, f1) - Getfromto(hue, f3, f4);
        float blue = Getfromto(hue, f2, f3) - Getfromto(hue, f5, 1F);
        return new Color(red, green, blue, alpha);
    }

    /**
	 * angle in radians
	 * 
	 */
    public static Vector3 RotateVecAroundAxis(Vector3 vector, Vector3 axisVector, float angle)//cross pr ���� x     dot pr ���� *
    {
        axisVector = axisVector.normalized;
        float cosangle = Mathf.Cos(angle);

        float dot = Vector3.Dot(axisVector, vector);
        Vector3 cross = Vector3.Cross(axisVector, vector);

        return (axisVector * (dot * (1 - cosangle))) + (cross * (Mathf.Sin(angle))) + (vector * (cosangle));
    }


    public static RaycastHit NormalizeRayTraceResult(RaycastHit hit, float offset)
    {
        hit.point = hit.point + hit.normal * offset;
        return hit;

    }



    //ActionSandboxRoguelong


    public static int Floor(float num)
    {
        return (int)Math.Floor(num);
    }
    public static int Floor(double num)
    {
        return (int)Math.Floor(num);
    }

    public static Transform transformWith(Vector3 position, Vector3 eulerAngles, Vector3 scale)
    {
        freeUsableTransform.position = position;
        freeUsableTransform.eulerAngles = eulerAngles;
        freeUsableTransform.localScale = scale;
        return freeUsableTransform;
    }

    public static float circleFormula(float x)
    {
        x -= 0.5F;
        return Mathf.Sqrt(0.25F - x * x) * 2F;
    }

    public static RaycastHit fixedRaycast(Vector3 origin, Vector3 direction, float maxDistance, int layerMask, float normalizeRayResult)
    {
        Physics.Raycast(origin, direction, out RaycastHit hitInfo, maxDistance, layerMask);

        if (hitInfo.collider != null)
        {
            hitInfo = NormalizeRayTraceResult(hitInfo, normalizeRayResult);
        }
        else
        {
            hitInfo.point = origin + direction * maxDistance;
            hitInfo.distance = maxDistance;
        }
        return hitInfo;
    }


    //public static Vector3 getPositionRelative(Vector3 relativeToPos, Vector3 relativeToUpVector, Vector3 point)
    //{
    //    freeUsableTransform.position = relativeToPos;
    //    freeUsableTransform.up = relativeToUpVector;
    //    return freeUsableTransform.InverseTransformPoint(point);
    //}
}

/*

Shader "Custom/Solid"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex("Albedo (RGB)", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _MainTex;

            struct Input
            {
                float2 uv_MainTex;
            };

            fixed4 _Color;

            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            UNITY_INSTANCING_BUFFER_START(Props)
                        // put more per-instance properties here
                    UNITY_INSTANCING_BUFFER_END(Props)

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // Albedo comes from a texture tinted by color
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }
            ENDCG
    }
            FallBack "Diffuse"
}

*/

