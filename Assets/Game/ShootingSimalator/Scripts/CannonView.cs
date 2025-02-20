using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;

public class CannonView : MonoBehaviour
{
    [SerializeField] private Transform trunkPoint;
    [SerializeField] private ProjectileView projectilePrefab;
    [SerializeField] private LineRenderer trajectoryRenderer;

    [field: SerializeField] public Transform BodyView { get; private set; }
    [field: SerializeField] public Transform TrunkView { get; private set; }

    [Header("Parametres"), SerializeField] private int maxBounces = 2;
    [SerializeField] private int trajectoryResolution = 100;
    [SerializeField] private int liftingLimit = 60;
    [SerializeField] private int loweringLimit = 15;


    [HideInInspector] public float FireForce = 50f;

    private void Update()
    {
        DrawTrajectory();
    }

    public IEnumerator AutomaticFire()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            Fire();
        }
    }
    public void Fire()
    {
        var projectile = Instantiate(projectilePrefab, trunkPoint.position, trunkPoint.rotation);
        projectile.Launch(trunkPoint.forward * FireForce, 0, maxBounces);

        TrunkView.DOLocalMoveZ(-0.2f, 0.1f).SetLoops(2, LoopType.Yoyo);
    }

    private void DrawTrajectory()
    {
        List<Vector3> points = ProjectileView.CalculateTrajectory(trunkPoint.position, trunkPoint.forward * FireForce, 0, 1, trajectoryResolution);
        points.Insert(0, trunkPoint.position);

        trajectoryRenderer.positionCount = points.Count;
        trajectoryRenderer.SetPositions(points.ToArray());
    }
    public void RotateTrunk(float force)
    {
        float currentAngle = TrunkView.transform.localEulerAngles.x;

        if (currentAngle > 180)
            currentAngle -= 360;

        float newAngle = Mathf.Clamp(currentAngle + force, -1 * liftingLimit, loweringLimit);

        TrunkView.transform.localEulerAngles = new Vector3(newAngle, TrunkView.transform.localEulerAngles.y, TrunkView.transform.localEulerAngles.z);
    }

    public void RotateBody(float force)
    {
        BodyView.transform.Rotate(Vector3.up, force);
    }
}