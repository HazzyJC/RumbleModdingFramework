using System.Reflection;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using AssetBundle = RumbleModdingFramework.Assetloading.AssetBundle;

namespace RumbleModdingFramework.AssetLoading
{
    class AssetLoader
    {
        public static GameObject menu;
        public static GameObject noti;
        public static GameObject spawnGun;
        public static AudioClip menuSelectClip;
        public static AudioClip notificationClip;

        public static void SpawnMenu(int num)
        {
            MelonLogger.Msg("Spawning menu");
            AssetBundle localAssetBundle = AssetBundle.LoadFromMemory(EmbeddedAssetBundle.LoadFromAssembly(Assembly.GetExecutingAssembly(), "RumbleModdingFramework.Resources.vrmenu.vm"));
            if (localAssetBundle == null)
            {
                MelonLogger.Msg("Failed");
                return;
            }
            var assetObject = localAssetBundle.LoadAsset<GameObject>("MenuPrefab");

            Transform spawnedNPC = Object.Instantiate(assetObject).transform;
            /*if (localAssetBundle == null)
            {
                MelonLogger.Msg("Failed");
                return;
            }

            foreach (var loadedAsset in localAssetBundle.LoadAllAssets())
            {
                if (loadedAsset.name.Equals("MenuPrefab"))
                {
                    Object.Instantiate(loadedAsset, new Vector3(0, 2000, 0), Quaternion.identity); 
                    menu = GameObject.Find("MenuPrefab(Clone)");
                }
            }*/
            menu = spawnedNPC.gameObject;
            GameObject.DontDestroyOnLoad(menu);
            localAssetBundle.Unload(false);
            if (GameObject.Find("MenuPrefab(Clone)") == null)
            {
                MelonLogger.Msg("Failed");
                SpawnMenu(1);
                return;
            }
            menu.gameObject.transform.Find("MenuHolder").Find("VRMenu").gameObject.AddComponent<MenuBehavior>();
            GameObject.Destroy(menu.gameObject.transform.Find("MenuHolder").Find("VRMenu").Find("Background").gameObject
                .GetComponent<BoxCollider>());
            menu.gameObject.transform.Find("MenuHolder").Find("VRMenu").Find("PrevPageButton").gameObject
                .AddComponent<ChangePageButton>();
            menu.gameObject.transform.Find("MenuHolder").Find("VRMenu").Find("NextPageButton").gameObject
                .AddComponent<ChangePageButton>();
            menu.gameObject.transform.Find("MenuHolder").Find("Button").gameObject.AddComponent<ButtonScript>();
        }

        // TODO: Add notifications
        /*public static void SpawnNotification(int num)
        {
    
            AssetBundle localAssetBundle = AssetBundle.LoadFromMemory(EmbeddedAssetBundle.LoadFromAssembly(Assembly.GetExecutingAssembly(), "HBMF.Resources.notification.nt"));
            if (localAssetBundle == null)
            {
                MelonLogger.Msg("Failed");
                return;
            }
            GameObject asset = localAssetBundle.LoadAsset<GameObject>("notification");
            noti = GameObject.Instantiate(asset, new Vector3(0, 2000, 0), Quaternion.identity);
            Notifications.sound = noti.AddComponent<AudioSource>();
            Notifications.sound.clip = notificationClip;
            localAssetBundle.Unload(false);
        }*/
    }
}