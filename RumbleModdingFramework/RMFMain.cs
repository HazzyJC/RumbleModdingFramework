using System;
using System.Collections;
using MelonLoader;
using RUMBLE.Managers;
using RumbleModdingFramework.AssetLoading;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Diagnostics;
using UnityEngine.Events;

namespace RumbleModdingFramework
{
    public static class BuildInfo
    {
        public const string Name = "RMF";
        public const string Author = "Hazzy";
        public const string Company = null;
        public const string Version = "0.0.1";
        public const string DownloadLink = null;
    }

    public class RMFMain : MelonMod
    {
        public static MelonPreferences_Category RMFCat;
        public static MelonPreferences_Entry<bool> ConfigRMF;
        private static bool debounce = false;
        
        private static bool initDone;
        
        private static bool isUIactive;

        private bool controllerDone;

        private static UnityEngine.InputSystem.InputActionMap xrmap = new InputActionMap("My XR Input Map");
        private static InputAction rightPrimary = xrmap.AddAction("Right Controller Primary Button");
        private static InputAction rightSecondary = xrmap.AddAction("Right Controller Secondary Button");

        private static InputAction rightGrip = xrmap.AddAction("Right Controller Grip Button");
        private static InputAction rightTrigger = xrmap.AddAction("Right Controller Trigger Button");

        private static InputAction rightJoystick = xrmap.AddAction("Right Controller Primary Joystick");

        private static InputAction leftPrimary = xrmap.AddAction("Left Controller Primary Button");
        private static InputAction leftGrip = xrmap.AddAction("Left Controller Grip Button");
        private static InputAction leftJoystick = xrmap.AddAction("Left Controller Primary Joystick");
        private static InputAction leftSecondary = xrmap.AddAction("Left Controller Secondary Button");
        private static InputAction leftTrigger = xrmap.AddAction("Left Controller Trigger Button");

        public static UnityEvent RMFInitialized;


        public override void OnLateInitializeMelon()
        {
            RMFInitialized = new UnityEvent();
        }
        

        private bool ran;
        private GetPlayerRefScriptRMFFUNCDONOTREF refScript = null;
        private string currentScene;
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            currentScene = sceneName;
            if (sceneName == "CustomArena0" || sceneName == "CustomArena1")
            {
                return;
            }
            if (sceneName == "Loader")
            {
                rightPrimary.AddBinding("<XRController>{RightHand}/primaryButton"); // primary works (float)
                rightSecondary.AddBinding("<XRController>{RightHand}/secondaryButton"); // secondary works (float)
                rightGrip.AddBinding("<XRController>{RightHand}/grip"); // grip works (float)
                rightTrigger.AddBinding("<XRController>{RightHand}/trigger"); // grip works (float)
                rightJoystick.AddBinding("<XRController>{RightHand}/primary2DAxis"); // primary joystick works (Vector2)
                
                leftPrimary.AddBinding("<XRController>{LeftHand}/primaryButton"); // primary works (float)
                leftGrip.AddBinding("<XRController>{LeftHand}/grip");
                leftJoystick.AddBinding("<XRController>{LeftHand}/primary2DAxis");
                leftSecondary.AddBinding("<XRController>{LeftHand}/secondaryButton");
                leftTrigger.AddBinding("<XRController>{LeftHand}/trigger");
                xrmap.Enable();
                MelonLogger.Msg("successfully loaded input");
                AssetLoader.SpawnMenu(1);
            }
            initDone = false;

            controllerDone = true;

            if (!ran)
            {
                GameObject playerManagerGO = GameObject.Find("Game Instance/Initializable/PlayerManager");
                if (playerManagerGO == null)
                {
                    MelonLogger.Msg("playerManagerGO is null");
                }
                refScript = playerManagerGO.AddComponent<GetPlayerRefScriptRMFFUNCDONOTREF>();
                ran = true;
            }
            else
            {
                stopwatch = Stopwatch.StartNew();
                refScript.OnSceneWasLoaded(sceneName);
            }
        }

        private Stopwatch stopwatch = new Stopwatch();

        private void AddRefs()
        {
            GameObjectRef.playerController = refScript.playerController;
            GameObjectRef.playerHead = refScript.playerHead;
            GameObjectRef.leftHand = refScript.leftHand;
            GameObjectRef.rightHand = refScript.rightHand;
            GameObjectRef.playerManager = refScript.playerManager;
                
            //GameObjectRef.enemyPlayerController = refScript.enemyPlayerController;


            if (GameObjectRef.playerController != null)
            {
                GameObjectRef.leftHandCollider = GameObjectRef.playerController.transform.Find("Physics").transform.Find("LeftPhysicsController")
                    .transform.Find("Collider").GetComponent<Collider>();
                GameObjectRef.rightHandCollider = GameObjectRef.playerController.transform.Find("Physics").transform.Find("RightPhysicsController")
                    .transform.Find("Collider").GetComponent<Collider>();

                var addedCollider = AssetLoader.menu.transform.Find("MenuHolder").Find("PlayerCollider").gameObject;
                addedCollider.layer = 31;
                var rumbleCollider = GameObjectRef.playerController.transform.Find("Visuals/RIG/Bone_Pelvis/Bone_Spine/Bone_Chest/Bone_Shoulderblade_R/Bone_Shoulder_R/Bone_Lowerarm_R/Bone_Hand_R/Bone_Pointer_A_R/Bone_Pointer_B_R/Bone_Pointer_C_R");
                GameObject.Instantiate(addedCollider, rumbleCollider.transform.position, rumbleCollider.transform.rotation, rumbleCollider.transform);

                VrMenu.menuObject = AssetLoader.menu.transform.Find("MenuHolder").Find("VRMenu").gameObject;
                var menuBehavior = VrMenu.menuObject.GetComponent<MenuBehavior>();
                menuBehavior.button = AssetLoader.menu.transform.Find("MenuHolder").Find("Button").gameObject;
                VrMenu.RefreshMenu();
                VrMenu.ShowPage(0);

                AssetLoader.menu.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                isUIactive = false;
                initDone = true;
                RMFInitialized.Invoke();
            }
        }

        private Stopwatch stopwatch2 = new Stopwatch();

        public override void OnUpdate()
        {
            if (stopwatch2.ElapsedMilliseconds >= 200 && stopwatch2 != null)
            {
                stopwatch2.Stop();
                stopwatch2.Reset();
                AddRefs();
            }
            if (stopwatch.ElapsedMilliseconds >= 200 && stopwatch != null)
            {
                stopwatch.Stop();
                stopwatch.Reset();
                refScript.OnSceneWasLoaded(currentScene);
                stopwatch2 = Stopwatch.StartNew();
            }
            if (initDone)
            {
                if (VrMenu.menuObject != null)
                {
                    var from = VrMenu.menuObject.transform.position;
                    var to = GameObjectRef.playerHead.transform.position;

                    VrMenu.menuObject.transform.eulerAngles = new Vector3(0f,
                        Quaternion.LookRotation(from - to).normalized.eulerAngles.y, 0f);
                }

                if (Input.RightSecButton)
                {
                    if (debounce == false)
                    {
                        debounce = true;
                        if (isUIactive)
                        {
                            isUIactive = false;
                        }
                        else
                        {
                            isUIactive = true;
                        }
                    }
                }
                else
                {
                    debounce = false;
                }

                if (isUIactive)
                    VrMenu.menuObject.transform.position =
                        GameObjectRef.leftHand.transform.position + new Vector3(0, 0.3f, 0);
                else
                    VrMenu.menuObject.transform.position = new Vector3(0, 3000, 0);
            }

            if (controllerDone)
            {
                Input.RightPrimButton = rightPrimary.ReadValue<float>() != 0; 
                Input.RightSecButton = rightSecondary.ReadValue<float>() != 0;
                Input.RightGripPress = rightGrip.ReadValue<float>() >= 0.5f;
                Input.RightGrip = rightGrip.ReadValue<float>();
                Input.RightTrigger = rightTrigger.ReadValue<float>();
                Input.RightTriggerPress = rightTrigger.ReadValue<float>() >= 0.5f;
                Input.RightJoyAxis = rightJoystick.ReadValue<Vector2>();

                Input.LeftPrimButton = leftPrimary.ReadValue<float>() != 0;
                Input.LeftSecButton = leftSecondary.ReadValue<float>() != 0;
                Input.LeftGripPress = leftGrip.ReadValue<float>() >= 0.5f;
                Input.LeftGrip = leftGrip.ReadValue<float>();
                Input.LeftTrigger = leftTrigger.ReadValue<float>();
                Input.LeftTriggerPress = leftTrigger.ReadValue<float>() >= 0.5f;
                Input.LeftJoyAxis = leftJoystick.ReadValue<Vector2>();
                Input.LeftGripPress = leftGrip.ReadValue<float>() >= 0.5f;
            }
        }
    }
    
    [RegisterTypeInIl2Cpp]
    public class Input : MonoBehaviour
    {
        public Input(IntPtr intPtr) : base(intPtr) { }
        public static bool LeftJoystickClick = false;
        public static bool RightJoystickClick = false;
        public static bool LeftPrimButton = false;
        public static bool RightPrimButton = false;
        public static bool LeftMenuButton = false;
        public static bool RightMenuButton = false;
        public static bool LeftSecButton = false;
        public static bool RightSecButton = false;
        public static bool LeftTrackpad = false;
        public static bool RightTrackPad = false;
        public static bool LeftTriggerPress = false;
        public static bool RightTriggerPress = false;
        public static Vector3 LeftVelocity;
        public static Vector3 RightVelocity;
        public static Vector2 LeftJoyAxis;
        public static Vector2 RightJoyAxis;
        public static float LeftTrigger;
        public static float RightTrigger;
        public static float LeftGrip;
        public static float RightGrip;
        public static bool LeftGripPress;
        public static bool RightGripPress;
    }

    [RegisterTypeInIl2Cpp]
    public class GameObjectRef : MonoBehaviour
    {
        public GameObjectRef(IntPtr intPtr) : base(intPtr) { }
        public static PlayerManager playerManager;
        public static GameObject playerController;
        public static GameObject playerHead;
        public static GameObject leftHand;
        public static GameObject rightHand;
        public static Collider leftHandCollider;
        public static Collider rightHandCollider;
        
        public static GameObject enemyPlayerController;
    }

    [RegisterTypeInIl2Cpp]
    public class GetPlayerRefScriptRMFFUNCDONOTREF : MonoBehaviour
    {
        public GetPlayerRefScriptRMFFUNCDONOTREF(IntPtr intPtr) : base(intPtr) { }
        
        public PlayerManager playerManager;
        public GameObject playerController;
        public GameObject playerHead;
        public GameObject leftHand;
        public GameObject rightHand;

        private GameObject enemyPlayerController;

        private bool refsDone = false;

        public void Start()
        {
            playerManager = gameObject.GetComponent<PlayerManager>();
        }

        public void OnSceneWasLoaded(string sceneName)
        {
            refsDone = false;
            if (sceneName != "Loader")
            {
                MelonCoroutines.Start(GetPlayerObjs());
            }
        }

        /*IEnumerator GetEnemyObjs()
        {
            yield return new WaitForSeconds(1);
            try
            {
                foreach (var player in playerManager.AllPlayers)
                {
                    if (playerManager.AllPlayers.Count == 2)
                    {
                        if (player != playerManager.LocalPlayer)
                        {
                            enemyPlayerController = player.Controller.gameObject;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MelonLogger.Msg("failed to get enemy objs");
            }
            MelonLogger.Msg("adding refs");
            refsDone = true;
        }*/
        private IEnumerator GetPlayerObjs()
        {
            yield return new WaitForSeconds(1);
            try
            {
                playerController = playerManager.LocalPlayer.Controller.gameObject;
                playerHead = playerController.transform.Find("Physics").transform.Find("PhysicsHeadset").gameObject;
                leftHand = playerController.transform.Find("Physics").transform.Find("LeftPhysicsController").gameObject;
                rightHand = playerController.transform.Find("Physics").transform.Find("RightPhysicsController").gameObject;
            }
            catch (Exception e)
            {
                MelonLogger.Msg(e.ToString());
                MelonCoroutines.Start(GetPlayerObjs());
            }
            
            refsDone = true;
            //MelonCoroutines.Start(GetEnemyObjs());
        }
    }
}