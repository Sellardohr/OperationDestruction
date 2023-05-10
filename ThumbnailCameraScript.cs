using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ThumbnailCameraScript : MonoBehaviour
{

    public string designSlot = "1";
    public RenderTexture textureToRenderTo;
    public string filepathToSaveTo;
    public DesignsManagerScript designsManager;

    // Start is called before the first frame update
    void Start()
    {
        designsManager = FindObjectOfType<DesignsManagerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string ChooseFilePathToSaveTo()
    {
        filepathToSaveTo = Path.Combine(designsManager.mechDesignRootFilepath, "DesignImage" + designSlot + ".png");
        return filepathToSaveTo;
    }

    public RenderTexture ChooseTextureToRenderTo()
    {
        var textureString = "Design Image " + designSlot;
        var textureStringPath = "Design Image Thumbnails/" + textureString;
        return Resources.Load<RenderTexture>(textureStringPath);
    }

    public void SaveMechThumbnail()
    {
        Camera thumbnailCam = GetComponent<Camera>();

        var currentRT = RenderTexture.active;
        RenderTexture saveSlotTexture = ChooseTextureToRenderTo();
        thumbnailCam.targetTexture = saveSlotTexture;
        RenderTexture.active = saveSlotTexture;

        designsManager.currentlyEditedMech.transform.Translate(new Vector2(0, 10));

        thumbnailCam.Render();

        Texture2D mechImage = new Texture2D(thumbnailCam.targetTexture.width, thumbnailCam.targetTexture.height);
        mechImage.ReadPixels(new Rect(0, 0, thumbnailCam.targetTexture.width, thumbnailCam.targetTexture.height), 0, 0);
        mechImage.Apply();
        RenderTexture.active = currentRT;

        var Bytes = mechImage.EncodeToPNG();
        Destroy(mechImage);

        designsManager.currentlyEditedMech.transform.Translate(new Vector2(0, -10));

        File.WriteAllBytes(ChooseFilePathToSaveTo(), Bytes);
    }

}
