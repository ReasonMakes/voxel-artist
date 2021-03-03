using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //Movement
    private Vector3 moveVector;
    private float moveSpeed = 10f;

    //Settings
    private readonly float MOUSE_SENS_COEFF = 1f;
    private float mouseSensitivity = 0.6f;

    //Camera
    private float hFieldOfView = 103f;
    private const float H_FIELD_OF_VIEW_MIN = 80f;
    private const float H_FIELD_OF_VIEW_MAX = 103f;
    private float fpCamPitch = 20f;
    private float fpCamYaw = 20f;
    private float fpCamRoll = 0f;

    //Voxel manipulation
    public World world;
    public Chunk chunk;
    public GameObject canvas;
    private Vector3 canvasPos;
    public Transform highlightVoxelPlace;
    public Transform highlightVoxelDestroy;
    private readonly float RAYCAST_INCREMENT = 0.01f;
    private readonly float REACH = 8f;
    public Image colourDisplayFrontImage;
    public byte  colourDisplayFrontPaletteIndex = 1;
    public Image colourDisplayBackImage;
    public byte  colourDisplayBackPaletteIndex = 2;
    //public Texture2D palette;
    //public Material paletteMat;
    private bool isColourPickerOpen = false;
    public GameObject colourPicker;
    private GameObject paletteHUD;
    private float paletteHUDCentre;
    private float paletteChildSize = 48f;
    private float paletteChildPaddingMultiplier = 1.5f;
    private byte paletteIndex = 1;

    //Data
    [System.NonSerialized] public static string userDataFolder = "/user";
    [System.NonSerialized] public static string screenshotsFolder = "/screenshots";
    [System.NonSerialized] public static string paletteFolder = "/palette";
    [System.NonSerialized] public static string paletteFilename = "/palette.png";

    private void Awake()
    {
        //Canvas
        canvasPos = canvas.GetComponent<RectTransform>().position;

        //Children init
        GameObject paletteChild;
        RectTransform paletteChildRectTransform;
        Image paletteChildImage;

        //Palette Selection Indicator child of canvas)
        paletteChild = new GameObject("Selected");
        paletteChild.transform.SetParent(canvas.transform);

        paletteChildRectTransform = paletteChild.AddComponent<RectTransform>();
        paletteChildRectTransform.position = canvasPos;
        paletteChildRectTransform.sizeDelta = new Vector2(paletteChildSize * (((paletteChildPaddingMultiplier - 1f) * 0.5f) + 1f), 4f);
        paletteChildRectTransform.pivot = new Vector2(0.5f, 0.5f);
        paletteChildRectTransform.position = new Vector3(paletteChildRectTransform.position.x, (paletteChildSize * 0.5f) - 4f, 0f);

        paletteChildImage = paletteChild.AddComponent<Image>();
        //paletteChildImage.color = chunk.palette[x];

        //Palette HUD
        paletteHUD = new GameObject("Palette HUD");
        paletteHUD.transform.SetParent(canvas.transform);
        
        RectTransform paletteHUDRectTransform = paletteHUD.AddComponent<RectTransform>();
        paletteHUDRectTransform.position = canvasPos;
        paletteHUDRectTransform.anchorMin = new Vector2(0.5f, 0f);
        paletteHUDRectTransform.anchorMax = new Vector2(0.5f, 0f);

        paletteHUDCentre = paletteHUD.GetComponent<RectTransform>().position.x;

        //Palette HUD Children
        for (byte x = 1; x < chunk.palette.Length; x++)
        {
            paletteChild = new GameObject("Palette " + x);
            paletteChild.transform.SetParent(paletteHUD.transform);

            paletteChildRectTransform = paletteChild.AddComponent<RectTransform>();
            paletteChildRectTransform.position = canvasPos;
            paletteChildRectTransform.sizeDelta = new Vector2(paletteChildSize, paletteChildSize);
            paletteChildRectTransform.pivot = new Vector2(0.5f, 0.5f);
            paletteChildRectTransform.position = new Vector3(paletteChildRectTransform.position.x + (x * paletteChildSize * paletteChildPaddingMultiplier), paletteChildSize, 0f);

            paletteChildImage = paletteChild.AddComponent<Image>();
            paletteChildImage.color = chunk.palette[x];
        }

        //Update display
        ScrollThroughPalette();
    }

    void Start()
    {
        //Camera HFOV
        if (hFieldOfView > H_FIELD_OF_VIEW_MIN && hFieldOfView < H_FIELD_OF_VIEW_MAX)
        {
            //Camera class's fieldOfView field stores VERTICAL field of view, so we convert h to v
            Camera.main.fieldOfView = Camera.HorizontalToVerticalFieldOfView(hFieldOfView, Camera.main.GetComponent<Camera>().aspect);
        }

        //Clip planes
        Camera.main.nearClipPlane = 0.05f;
        Camera.main.farClipPlane = 500f;
    }

    private void Update()
    {
        UpdateCameraView();
        UpdateVoxelManipulation();
        UpdatePalette();

        if (Input.GetKeyDown(KeyCode.F2))
        {
            SaveScreenshot();
        }
    }

    void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateCameraView()
    {
        if (isColourPickerOpen)
        {
            return;
        }

        //Pitch
        fpCamPitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * MOUSE_SENS_COEFF;
        //Yaw
        if (fpCamPitch >= 90 && fpCamPitch < 270)
        {
            //Normal
            fpCamYaw -= Input.GetAxisRaw("Mouse X") * mouseSensitivity * MOUSE_SENS_COEFF;
        }
        else
        {
            //Inverted
            fpCamYaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity * MOUSE_SENS_COEFF;
        }
        //Roll
        fpCamRoll = 0f;

        //LOOP ANGLE
        LoopEulerAngle(fpCamYaw);
        LoopEulerAngle(fpCamPitch);
        LoopEulerAngle(fpCamRoll);

        //CLAMP ANGLE
        fpCamPitch = Mathf.Clamp(fpCamPitch, -90f, 89f);

        //APPLY ROTATION
        Camera.main.transform.localRotation = Quaternion.Euler(fpCamPitch, fpCamYaw, 0f);
    }

    private void UpdateVoxelManipulation()
    {
        //POSITION VOXEL HIGHLIGHTS
        float step = 0;
        Vector3 lastPos = transform.position;

        while (step < REACH)
        {
            Vector3 pos = transform.position + (transform.forward * step);
            int posX = Mathf.FloorToInt(pos.x);
            int posY = Mathf.FloorToInt(pos.y);
            int posZ = Mathf.FloorToInt(pos.z);

            //if (world.CheckForVoxel(pos))
            if (!Chunk.AreCoordsOutOfChunk(posX, posY, posZ) && chunk.voxelInfo[posX, posY, posZ] != 0)
            {
                highlightVoxelDestroy.gameObject.SetActive(true);
                highlightVoxelPlace.gameObject.SetActive(true);

                highlightVoxelDestroy.position = new Vector3(posX, posY, posZ);

                highlightVoxelPlace.position = lastPos;

                //highlightVoxelDestroy.gameObject.SetActive(true);
                //highlightVoxelPlace.gameObject.SetActive(true);

                step = REACH;
            }
            else
            {
                highlightVoxelDestroy.gameObject.SetActive(false);
                highlightVoxelPlace.gameObject.SetActive(false);

                lastPos = new Vector3(
                    Mathf.FloorToInt(pos.x),
                    Mathf.FloorToInt(pos.y),
                    Mathf.FloorToInt(pos.z)
                );

                step += RAYCAST_INCREMENT;
            }
        }

        //EDIT VOXEL AT HIGHLIGHT
        if (highlightVoxelDestroy.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                world.chunk.SetVoxelInfo(highlightVoxelDestroy.position, 0);
            }

            if (Input.GetMouseButtonDown(1))
            {
                //Debug.Log(colourDisplayFrontPaletteIndex);
                world.chunk.SetVoxelInfo(highlightVoxelPlace.position, colourDisplayFrontPaletteIndex);
                /*
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    world.chunk.SetVoxelInfo(highlightVoxelPlace.position, colourSecondary);
                }
                else
                {
                    world.chunk.SetVoxelInfo(highlightVoxelPlace.position, colourPrimary);
                }
                */
            }
        }
    }
    private void UpdatePalette()
    {
        //Swap primary/secondary colours
        if (Input.GetKeyDown(KeyCode.X))
        {
            byte frontOld = colourDisplayFrontPaletteIndex;
            colourDisplayFrontPaletteIndex = colourDisplayBackPaletteIndex;
            colourDisplayBackPaletteIndex = frontOld;
        }

        //Modify colours on the palette
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isColourPickerOpen = !isColourPickerOpen;
            colourPicker.SetActive(isColourPickerOpen);

            if (isColourPickerOpen)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (isColourPickerOpen)
        {
            Vector3[] corners = new Vector3[4];
            Image colourPickerImage = colourPicker.GetComponent<Image>();
            colourPickerImage.rectTransform.GetWorldCorners(corners);
            Rect newRect = new Rect(corners[0], corners[2] - corners[0]);

            if (newRect.Contains(Input.mousePosition) && Input.GetMouseButton(0))
            {
                //Get
                int posX = Mathf.FloorToInt(Input.mousePosition.x - (colourPicker.transform.position.x - colourPickerImage.sprite.texture.width / 2f));
                int posY = Mathf.FloorToInt(Input.mousePosition.y - (colourPicker.transform.position.y - colourPickerImage.sprite.texture.height / 2f));

                Color colourPicked = colourPickerImage.sprite.texture.GetPixel(
                    posX,
                    posY
                );
                
                colourPicked.a = 1f;

                //Debug.Log(colourPicked + "\n" + posX + ", " + posY);
                //Debug.Log(colourDisplayFrontPaletteIndex);

                //Set
                chunk.palette[colourDisplayFrontPaletteIndex] = colourPicked;
                UpdatePaletteDisplay();
                chunk.UpdateMaterial();


                //palette.SetPixel(colourDisplayFrontPaletteIndex, 0, colourPicked);
                //palette.Apply();
                //
                ////I/O
                //EnsureDirectoryExists(Application.persistentDataPath + userDataFolder);
                //EnsureDirectoryExists(Application.persistentDataPath + userDataFolder + paletteFolder);
                //
                //SavePalette();
                //
                //LoadPalette();
            }
        }

        //Scroll through palette
        if (Input.mouseScrollDelta.y != 0)
        {
            ScrollThroughPalette();
        }

        //Select colour from palette
        //if (Input.GetMouseButtonDown(2))
        //{
        //    colourDisplayFrontPaletteIndex = paletteIndex;
        //}

        //Display colours
        colourDisplayFrontImage.color = chunk.palette[colourDisplayFrontPaletteIndex];
        colourDisplayBackImage.color = chunk.palette[colourDisplayBackPaletteIndex];

        //colourDisplayFrontImage.color = palette.GetPixel(colourDisplayFrontPaletteIndex, 0);
        //colourDisplayBackImage.color = palette.GetPixel(colourDisplayBackPaletteIndex, 0);
    }

    private void ScrollThroughPalette()
    {
        paletteIndex = (byte)Mathf.Min(chunk.palette.Length - 1f, Mathf.Max(1f, paletteIndex - Input.mouseScrollDelta.y));

        paletteHUD.GetComponent<RectTransform>().position = new Vector3(
            paletteHUDCentre + (-paletteIndex * paletteChildSize * paletteChildPaddingMultiplier),
            0f,
            0f
        );

        colourDisplayFrontPaletteIndex = paletteIndex;
        //Debug.Log(paletteIndex);
    }

    private void UpdatePaletteDisplay()
    {
        GameObject paletteChild;
        Image paletteChildImage;

        for (byte x = 1; x < chunk.palette.Length; x++)
        {
            paletteChild = GameObject.Find("Palette " + x);
            paletteChildImage = paletteChild.GetComponent<Image>();
            paletteChildImage.color = chunk.palette[x];
        }
    }

    //private void SavePalette()
    //{
    //    byte[] bytes = palette.EncodeToPNG();
    //
    //    string path = Application.persistentDataPath + userDataFolder + paletteFolder + paletteFilename;
    //    File.WriteAllBytes(path, bytes);
    //}
    //
    //private void LoadPalette()
    //{
    //    string path = Application.persistentDataPath + userDataFolder + paletteFolder + paletteFilename;
    //
    //    byte[] bytes = File.ReadAllBytes(path);
    //
    //    palette.LoadImage(bytes);
    //
    //    /*
    //    for(int i = 0; i < palette.width; i++)
    //    {
    //        Debug.Log("Pixel " + i + ": " + palette.GetPixel(i, 0));
    //    }
    //    */
    //
    //    //paletteMat.SetTexture("", palette);
    //    //chunk.gameObject.GetComponent<Renderer>().material.mainTexture = palette;
    //    //chunk.UpdateMaterial(palette);
    //}

    private void UpdateMovement()
    {
        moveVector = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveVector += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveVector += -transform.forward;
        if (Input.GetKey(KeyCode.A)) moveVector += -transform.right;
        if (Input.GetKey(KeyCode.D)) moveVector += transform.right;
        if (Input.GetKey(KeyCode.Space)) moveVector += transform.up;
        if (Input.GetKey(KeyCode.LeftControl)) moveVector += -transform.up;

        if (moveVector != Vector3.zero)
        {
            transform.position += moveVector.normalized * moveSpeed * Time.fixedDeltaTime;
        }
    }

    public void SaveScreenshot()
    {
        EnsureDirectoryExists(Application.persistentDataPath + userDataFolder);
        EnsureDirectoryExists(Application.persistentDataPath + userDataFolder + screenshotsFolder);

        //Generate the filename based on time of screenshot
        //We use string formatting to ensure there are leading zeros to help system file explorers accurately sort
        string path = Application.persistentDataPath + userDataFolder + screenshotsFolder
            + "/" + System.DateTime.Now.Year
            + "-" + System.DateTime.Now.Month.ToString("d2")
            + "-" + System.DateTime.Now.Day.ToString("d2")
            + "_" + System.DateTime.Now.Hour.ToString("d2")
            + "-" + System.DateTime.Now.Minute.ToString("d2")
            + "-" + System.DateTime.Now.Second.ToString("d2")
            + "-" + System.DateTime.Now.Millisecond.ToString("d4")
            + ".png";

        ScreenCapture.CaptureScreenshot(path);
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Debug.Log("Directory does not exist; creating directory: " + path);
            Directory.CreateDirectory(path);
        }
    }

    private float LoopEulerAngle(float angle)
    {
        if (angle >= 360) angle -= 360;
        else if (angle < 0) angle += 360;

        return angle;
    }
}