using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProjectileView : MonoBehaviour
{
    public const float Gravity = 9.81f;
    public const float Drag = 0.98f;
    private const float TimeStep = 0.02f;
    private const float EnergyLossOnRebound = 0.8f;

    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Texture2D impactTexture;
    [SerializeField] private ParticleSystem particle;

    private Vector3 velocity;
    private Vector3 position;
    private int bouncesLeft;

    public virtual void Launch(Vector3 initialVelocity, float angle, int maxBounces)
    {
        velocity = Quaternion.Euler(-angle, 0, 0) * initialVelocity;
        position = transform.position;
        bouncesLeft = maxBounces;
        GenerateRandomMesh();
    }

    private void Update()
    {
        if (bouncesLeft < 0) return;

        SimulateTrajectoryStep(ref position, ref velocity, ref bouncesLeft, (hit, newVelocity) =>
        {
            velocity = newVelocity;
            CreateImpactEffect(hit.point, hit.normal);

            if (bouncesLeft <= 0)
            {
                Explode();
            }
        });

        transform.position = position;
    }

    private static void SimulateTrajectoryStep(ref Vector3 position, ref Vector3 velocity, ref int bounces, System.Action<RaycastHit, Vector3> onBounce)
    {
        Vector3 newPosition = position + velocity * TimeStep;
        velocity *= Drag;
        velocity += Vector3.down * Gravity * TimeStep;

        if (Physics.Raycast(position, newPosition - position, out RaycastHit hit, (newPosition - position).magnitude))
        {
            position = hit.point;
            Vector3 newVelocity = Vector3.Reflect(velocity, hit.normal) * EnergyLossOnRebound;
            bounces--;

            onBounce?.Invoke(hit, newVelocity);
        }
        else
        {
            position = newPosition;
        }
    }

    protected virtual void CreateImpactEffect(Vector3 position, Vector3 normal)
    {
        if (Physics.Raycast(position + normal * 0.01f, -normal, out RaycastHit hit, 0.02f))
        {
            WallView wall = hit.collider.GetComponent<WallView>();
            if (wall != null)
            {
                wall.ApplyImpactTexture(impactTexture, hit);
            }
        }
    }

    protected virtual void Explode()
    {
        var particleInst = Instantiate(particle, transform.position, Quaternion.identity);
        particleInst.Play();
        Destroy(particleInst.gameObject, particleInst.main.duration);

        Camera.main.transform.DOShakePosition(0.4f, 0.2f, 10, 90, false, true); //TODO rework later
        
        Destroy(gameObject);
    }

    public static List<Vector3> CalculateTrajectory(Vector3 startPosition, Vector3 initialVelocity, float angle, int maxBounces, int resolution)
    {
        List<Vector3> points = new();
        Vector3 position = startPosition;
        Vector3 velocity = Quaternion.Euler(-angle, 0, 0) * initialVelocity;
        int bounces = maxBounces;

        for (int i = 0; i < resolution && bounces > 0; i++)
        {
            SimulateTrajectoryStep(ref position, ref velocity, ref bounces, (hit, newVelocity) =>
            {
                velocity = newVelocity;
            });

            points.Add(position);
        }

        return points;
    }

    private void GenerateRandomMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = {
            new Vector3(-0.5f + RandomOffset(), -0.5f + RandomOffset(), -0.5f + RandomOffset()),
            new Vector3(0.5f + RandomOffset(), -0.5f + RandomOffset(), -0.5f + RandomOffset()),
            new Vector3(0.5f + RandomOffset(), 0.5f + RandomOffset(), -0.5f + RandomOffset()),
            new Vector3(-0.5f + RandomOffset(), 0.5f + RandomOffset(), -0.5f + RandomOffset()),
            new Vector3(-0.5f + RandomOffset(), -0.5f + RandomOffset(), 0.5f + RandomOffset()),
            new Vector3(0.5f + RandomOffset(), -0.5f + RandomOffset(), 0.5f + RandomOffset()),
            new Vector3(0.5f + RandomOffset(), 0.5f + RandomOffset(), 0.5f + RandomOffset()),
            new Vector3(-0.5f + RandomOffset(), 0.5f + RandomOffset(), 0.5f + RandomOffset())
        };

        int[] triangles = {
            0, 2, 1, 0, 3, 2,
            4, 5, 6, 4, 6, 7,
            0, 7, 3, 0, 4, 7,
            1, 2, 6, 1, 6, 5,
            0, 1, 5, 0, 5, 4,
            3, 7, 6, 3, 6, 2
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

        float RandomOffset() => Random.Range(-0.1f, 0.1f);
    }
}
