using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMgr : MonoBehaviour
{
    public static EntityMgr inst;
    private void Awake()
    {
        inst = this;
        entities = new List<Entity>();
        foreach (Entity ent in movableEntitiesRoot.GetComponentsInChildren<Entity>())
        {
            entities.Add(ent);
        }
    }

    private void Start()
    {
        // GameMgr.OnGameStarted += this.OnGameStarted;
        GameMgr.OnGameStopped += () => this.DestroyAllEntities();
    }

    public void OnGameStarted()
    {
        Physics.SyncTransforms();

        int numEntities = GameMgr.Mode switch
        {
            Mode.WayPointGeneration => 1,
            Mode.SingleEntityWaypointFollow => 5,
            Mode.GroupMovementPotentialField => 10,
            Mode.AStarPotentialField => 10,
            _ => 0
        };

        Vector3 startPos = GameMgr.Environment switch
        {
            Environment.Office => new Vector3(10, 0, 175),
            _ => Vector3.zero
        };

        for (int i = 0; i < numEntities; i++)
        {
            Vector3 newStart;

            // Create entities in a line
            if (GameMgr.Environment == Environment.Office)
                newStart = startPos + new Vector3(5 * i, 0, 5 * i);

            // Use start area
            else
                newStart = GetRandomPointInBounds(EnvironmentMgr.inst.startArea.GetComponent<Collider>());

            // Random rotation
            Vector3 rotation = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);

            CreateEntity(EntityType.TugBoat, newStart, rotation);
        }
    }

    private Vector3 GetRandomPointInBounds(Collider col)
    {
        Bounds b = col.bounds;

        // Pick random coordinates within the bounding box
        float x = UnityEngine.Random.Range(b.min.x, b.max.x);
        float y = UnityEngine.Random.Range(b.min.y, b.max.y);
        float z = UnityEngine.Random.Range(b.min.z, b.max.z);

        return new Vector3(x, y, z);
    }

    public GameObject movableEntitiesRoot;
    public List<GameObject> entityPrefabs;
    public GameObject entitiesRoot;
    public List<Entity> entities;

    public static int entityId = 0;

    public Entity CreateEntity(EntityType et, Vector3 position, Vector3 eulerAngles)
    {
        Entity entity = null;
        GameObject entityPrefab = entityPrefabs.Find(x => x.GetComponent<Entity>().entityType == et);
        if (entityPrefab != null)
        {
            GameObject entityGo = Instantiate(entityPrefab, position, Quaternion.Euler(eulerAngles), entitiesRoot.transform);
            if (entityGo != null)
            {
                entity = entityGo.GetComponent<Entity>();
                entity.desiredHeading = eulerAngles.y;
                entity.isSelected = false;
                entityGo.name = et.ToString() + entityId++;
                entities.Add(entity);
            }
        }
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (this.entities.Remove(entity))
            Destroy(entity.gameObject);
    }

    public void DestroyAllEntities()
    {
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            entities.RemoveAt(i);
            Destroy(entity.gameObject);
        }
    }
}
