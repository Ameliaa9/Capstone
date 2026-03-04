using UnityEngine;
using System.Collections.Generic;

public class NPCPuppetMaster : MonoBehaviour
{
    [Header("1. Master NPCs (The Templates)")]
    public List<GameObject> masterNPCs = new List<GameObject>();

    [Header("2. Target Folders (The Drivers)")]
    public List<Transform> foldersToScan = new List<Transform>();

    [Header("Visibility Settings")]
    public bool showSplineDrivers = false;
    private bool lastVisibilityState;

    private GameObject maleMaster;
    private GameObject femaleMaster;

    private class NPCPair
    {
        public Transform driver;
        public Transform puppet;
        public List<Renderer> driverRenderers;

        // This is where we store your specific animation script
        // Change 'YourWalkingScript' to the actual name of your script (e.g., PedestrianController)
        // public YourWalkingScript animationScript; 
    }

    private List<NPCPair> activeNPCs = new List<NPCPair>();
    private HashSet<Transform> processedDrivers = new HashSet<Transform>();

    void Start()
    {
        lastVisibilityState = showSplineDrivers;
        IdentifyMasters();

        foreach (Transform folder in foldersToScan)
        {
            if (folder == null) continue;
            Transform[] allInFolder = folder.GetComponentsInChildren<Transform>(true);

            foreach (Transform driver in allInFolder)
            {
                if (driver == folder || processedDrivers.Contains(driver) || IsMaster(driver.gameObject))
                    continue;

                string cleanName = driver.name.ToLower().Replace(" ", "");
                GameObject template = null;

                if (cleanName.Contains("female")) template = femaleMaster;
                else if (cleanName.Contains("male")) template = maleMaster;

                if (template != null) SetupPuppet(driver, template);
            }
        }
    }

    void IdentifyMasters()
    {
        foreach (GameObject go in masterNPCs)
        {
            if (go == null) continue;
            string n = go.name.ToLower();
            if (n.Contains("female")) femaleMaster = go;
            else if (n.Contains("male")) maleMaster = go;
        }
    }

    bool IsMaster(GameObject go) { return masterNPCs.Contains(go); }

    void SetupPuppet(Transform driver, GameObject template)
    {
        List<Renderer> rands = new List<Renderer>();
        foreach (Renderer r in driver.GetComponentsInChildren<Renderer>())
        {
            rands.Add(r);
            r.enabled = showSplineDrivers;
        }

        GameObject puppetGO = Instantiate(template, driver.position, driver.rotation);
        puppetGO.name = "Puppet_" + driver.name;
        puppetGO.transform.SetParent(this.transform);

        NPCPair newPair = new NPCPair
        {
            driver = driver,
            puppet = puppetGO.transform,
            driverRenderers = rands
        };

        // CACHE YOUR SCRIPT HERE:
        // newPair.animationScript = puppetGO.GetComponent<YourWalkingScript>();

        activeNPCs.Add(newPair);
        processedDrivers.Add(driver);
    }

    void Update()
    {
        if (showSplineDrivers != lastVisibilityState)
        {
            UpdateDriverVisibility();
            lastVisibilityState = showSplineDrivers;
        }
    }

    void UpdateDriverVisibility()
    {
        foreach (var pair in activeNPCs)
        {
            if (pair.driverRenderers == null) continue;
            foreach (Renderer r in pair.driverRenderers) if (r != null) r.enabled = showSplineDrivers;
        }
    }

    void LateUpdate()
    {
        foreach (var pair in activeNPCs)
        {
            if (pair.driver == null || pair.puppet == null) continue;

            // 1. Calculate Synthetic Speed
            // We measure how far the puppet has to "jump" to catch the driver
            float distance = Vector3.Distance(pair.puppet.position, pair.driver.position);
            float currentSpeed = distance / Time.deltaTime;

            // 2. Micro-Teleport
            pair.puppet.position = pair.driver.position;
            pair.puppet.rotation = pair.driver.rotation;

            // 3. FEED THE ANIMATION SCRIPT
            // Replace 'speedVariable' with whatever your script uses (e.g., moveSpeed, velocity, etc.)
            /*
            if (pair.animationScript != null)
            {
                pair.animationScript.speedVariable = currentSpeed;
            }
            */
        }
    }
}