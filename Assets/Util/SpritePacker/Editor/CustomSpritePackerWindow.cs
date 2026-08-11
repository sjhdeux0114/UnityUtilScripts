#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class CustomSpritePackerWindow : EditorWindow
{
    private List<Texture2D> texturesToPack = new List<Texture2D>();
    private string atlasName = "NewSpriteAtlas";
    private int maxAtlasSize = 4096;
    private int padding = 2;
    private bool deleteOriginals = false;

    private Vector2 scrollPos;

    // 추가되는 텍스처들 외에, 기존 아틀라스에서 추출된 스프라이트 메타데이터를 개별 보존하기 위한 딕셔너리
    private Dictionary<Texture2D, SpriteMetaInfo> preExtractedMeta = new Dictionary<Texture2D, SpriteMetaInfo>();

    [MenuItem("Tools/Custom Sprite Packer")]
    public static void ShowWindow()
    {
        var window = GetWindow<CustomSpritePackerWindow>("Sprite Packer");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Atlas Packer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("여러 개의 스프라이트 이미지를 선택하여 하나의 아틀라스로 병합하고, 자동으로 슬라이스(분할)합니다. 병합된 후 원본 이미지를 삭제하여 프로젝트 용량을 절약할 수 있습니다.", MessageType.Info);

        GUILayout.Space(10);
        atlasName = EditorGUILayout.TextField("Atlas Name (저장될 파일명)", atlasName);
        maxAtlasSize = EditorGUILayout.IntSlider("Max Atlas Size", maxAtlasSize, 256, 8192);
        padding = EditorGUILayout.IntSlider("Padding (여백)", padding, 0, 16);
        deleteOriginals = EditorGUILayout.Toggle("원본 파일 삭제하기", deleteOriginals);

        GUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("합칠 텍스처(스프라이트) 목록:");
        if (GUILayout.Button("목록 초기화", GUILayout.Width(100)))
        {
            texturesToPack.Clear();
            preExtractedMeta.Clear();
        }
        EditorGUILayout.EndHorizontal();

        // Drag and drop area
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 60.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "\n이곳에 스프라이트나 폴더를 드래그 앤 드롭 하세요.", EditorStyles.helpBox);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Texture2D texture)
                        {
                            string assetPath = AssetDatabase.GetAssetPath(texture);
                            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                            // 이미 아틀라스(Multiple)인 경우 내부 스프라이트들을 분리해서 리스트에 넣음
                            if (importer != null && importer.spriteImportMode == SpriteImportMode.Multiple)
                            {
                                ExtractSpritesFromAtlas(texture, importer);
                            }
                            else
                            {
                                if (!texturesToPack.Contains(texture))
                                    texturesToPack.Add(texture);
                            }
                        }
                        else if (draggedObject is DefaultAsset) // 폴더를 드래그한 경우
                        {
                            string path = AssetDatabase.GetAssetPath(draggedObject);
                            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });
                            foreach (string guid in guids)
                            {
                                string texPath = AssetDatabase.GUIDToAssetPath(guid);
                                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                                if (tex != null && !texturesToPack.Contains(tex))
                                {
                                    texturesToPack.Add(tex);
                                }
                            }
                        }
                    }
                }
                break;
        }

        // List selected textures
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(250));
        for (int i = 0; i < texturesToPack.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            texturesToPack[i] = (Texture2D)EditorGUILayout.ObjectField(texturesToPack[i], typeof(Texture2D), false);
            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                if (preExtractedMeta.ContainsKey(texturesToPack[i]))
                    preExtractedMeta.Remove(texturesToPack[i]);

                texturesToPack.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Space(15);
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Atlas 만들기 (Pack Sprites)", GUILayout.Height(40)))
        {
            if (texturesToPack.Count > 0)
            {
                PackSprites();
            }
            else
            {
                EditorUtility.DisplayDialog("경고", "합칠 텍스처를 하나 이상 목록에 추가해주세요.", "확인");
            }
        }
        GUI.backgroundColor = Color.white;
    }

    private void PackSprites()
    {
        texturesToPack.RemoveAll(t => t == null); // 빈 항목 제거

        string savePath = EditorUtility.SaveFilePanelInProject("아틀라스 저장 위치", atlasName, "png", "새로 만들어질 아틀라스를 저장할 위치를 선택하세요.");
        if (string.IsNullOrEmpty(savePath)) return;

        // 사용자가 다이얼로그 창에서 지정한 실제 파일명으로 텍스트 필드를 갱신합니다.
        atlasName = Path.GetFileNameWithoutExtension(savePath);

        Dictionary<string, bool> originalReadableStates = new Dictionary<string, bool>();
        Dictionary<string, TextureImporterFormat> originalFormats = new Dictionary<string, TextureImporterFormat>();
        Dictionary<string, TextureImporterCompression> originalCompressions = new Dictionary<string, TextureImporterCompression>();
        Dictionary<string, TextureImporterNPOTScale> originalNPOTScales = new Dictionary<string, TextureImporterNPOTScale>();

        List<Texture2D> readableTextures = new List<Texture2D>();
        List<string> originalPaths = new List<string>();
        List<SpriteMetaInfo> originalMetaDataList = new List<SpriteMetaInfo>();

        try
        {
            EditorUtility.DisplayProgressBar("Sprite Packer", "텍스처 준비 중...", 0.0f);

            // 1. Pack 하기 위해 모든 텍스처의 Read/Write 권한 활성화 및 비압축 설정
            for (int i = 0; i < texturesToPack.Count; i++)
            {
                Texture2D tex = texturesToPack[i];

                // 이미 아틀라스에서 추출해둔 메모리 텍스처인 경우 (물리 파일 없음)
                if (preExtractedMeta.ContainsKey(tex))
                {
                    readableTextures.Add(tex);

                    // 추출될 때 저장해둔 SpriteMetaInfo를 원본 메타데이터에 그대로 추가 (pivot, border, **GUID** 유지)
                    SpriteMetaInfo savedMeta = preExtractedMeta[tex];
                    originalMetaDataList.Add(new SpriteMetaInfo
                    {
                        name = savedMeta.name,
                        alignment = savedMeta.alignment,
                        pivot = savedMeta.pivot,
                        border = savedMeta.border,
                        spriteID = savedMeta.spriteID
                    });

                    EditorUtility.DisplayProgressBar("Sprite Packer", $"{tex.name} 준비 중...", (float)i / texturesToPack.Count * 0.3f);
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(tex);
                originalPaths.Add(path);

                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    originalReadableStates[path] = ti.isReadable;
                    originalCompressions[path] = ti.textureCompression;
                    originalNPOTScales[path] = ti.npotScale;

                    // 기존 스프라이트 설정(피벗, 보더 등) 백업
                    TextureImporterSettings settings = new TextureImporterSettings();
                    ti.ReadTextureSettings(settings);

                    SpriteMetaInfo meta = new SpriteMetaInfo
                    {
                        name = tex.name,
                        alignment = ti.spriteImportMode == SpriteImportMode.Multiple ? 0 : settings.spriteAlignment,
                        pivot = ti.spriteImportMode == SpriteImportMode.Multiple ? Vector2.zero : settings.spritePivot,
                        border = ti.spriteImportMode == SpriteImportMode.Multiple ? Vector4.zero : settings.spriteBorder,
                        spriteID = GUID.Generate().ToString() // 새로운 스프라이트는 고유 ID를 부여
                    };
                    originalMetaDataList.Add(meta);

                    // 읽기가 불가능하거나 압축되어있거나 NPOT 스케일이 켜져있으면 임시로 풀어준다
                    if (!ti.isReadable || 
                        ti.textureCompression != TextureImporterCompression.Uncompressed || 
                        ti.npotScale != TextureImporterNPOTScale.None)
                    {
                        ti.isReadable = true;
                        ti.textureCompression = TextureImporterCompression.Uncompressed;
                        ti.npotScale = TextureImporterNPOTScale.None;
                        ti.SaveAndReimport();
                    }
                }

                // 설정 변경 후 확실하게 다시 로드
                Texture2D reloadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                readableTextures.Add(reloadedTex);
                EditorUtility.DisplayProgressBar("Sprite Packer", $"{tex.name} 준비 중...", (float)i / texturesToPack.Count * 0.3f);
            }

            // 중복 이름 방지를 위한 처리
            HashSet<string> usedNames = new HashSet<string>();
            for (int i = 0; i < readableTextures.Count; i++)
            {
                var texName = readableTextures[i].name;
                string newName = texName;
                int suffix = 1;
                while (usedNames.Contains(newName))
                {
                    newName = $"{texName} ({suffix})";
                    suffix++;
                }
                usedNames.Add(newName);
                readableTextures[i].name = newName;

                SpriteMetaInfo mData = originalMetaDataList[i];
                mData.name = newName;
                originalMetaDataList[i] = mData; // 구조체/클래스 반영
            }

            int sheetIndex = 0;
            List<Texture2D> remainingTextures = new List<Texture2D>(readableTextures);
            List<SpriteMetaInfo> remainingMetas = new List<SpriteMetaInfo>(originalMetaDataList);

            bool successAll = true;

            while (remainingTextures.Count > 0)
            {
                EditorUtility.DisplayProgressBar("Sprite Packer", $"아틀라스 시트 {sheetIndex + 1} 생성 중...", 0.4f);

                Texture2D atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                // PackTextures는 실패하면 null을 반환합니다. 
                // 전체를 한 번에 패킹해보고 실패하면 개수를 줄여가며 시도하여 현재 시트에 최대한 많이 넣습니다.
                Rect[] uvs = null;
                int packCount = remainingTextures.Count;

                while (packCount > 0)
                {
                    Texture2D[] texturesToTry = remainingTextures.Take(packCount).ToArray();

                    // maximumAtlasSize를 주면 유니티가 억지로 욱여넣으면서 이미지를 축소(Downscale)시켜버리는 문제가 발생할 수 있습니다.
                    uvs = atlas.PackTextures(texturesToTry, padding, maxAtlasSize);

                    if (uvs != null)
                    {
                        // PackTextures가 성공했더라도 유니티가 임의로 이미지를 축소시켰는지 검사합니다.
                        bool isDownscaled = false;
                        for (int i = 0; i < texturesToTry.Length; i++)
                        {
                            int expectedWidth = texturesToTry[i].width;
                            int expectedHeight = texturesToTry[i].height;

                            int packedWidth = Mathf.RoundToInt(uvs[i].width * atlas.width);
                            int packedHeight = Mathf.RoundToInt(uvs[i].height * atlas.height);

                            // 만약 원본 사이즈보다 패킹된 픽셀 사이즈가 작다면 유니티가 강제로 축소한 것입니다.
                            if (packedWidth < expectedWidth || packedHeight < expectedHeight)
                            {
                                isDownscaled = true;
                                break;
                            }
                        }

                        if (!isDownscaled)
                        {
                            break; // 성공적으로 원본 해상도 그대로 패킹됨
                        }
                    }

                    packCount--; // 실패했거나 축소되었다면 하나 줄여서 다시 시도
                }

                if (packCount == 0 || uvs == null)
                {
                    // 단 한장의 이미지도 maxAtlasSize에 들어가지 못하는 경우
                    successAll = false;
                    Debug.LogError($"'{remainingTextures[0].name}' 이미지가 너무 커서 Max Atlas Size({maxAtlasSize})에 들어갈 수 없습니다.");
                    break;
                }

                // 3. PNG 생성 후 저장
                string currentSheetName = sheetIndex == 0 ? atlasName : $"{atlasName}_{sheetIndex}";
                string currentSavePath = savePath.Replace(Path.GetFileNameWithoutExtension(savePath), currentSheetName);

                EditorUtility.DisplayProgressBar("Sprite Packer", $"아틀라스 시트 {sheetIndex + 1} 파일 저장 중...", 0.6f);
                byte[] pngData = atlas.EncodeToPNG();
                File.WriteAllBytes(currentSavePath, pngData);
                AssetDatabase.Refresh();

                // 4. 저장된 아틀라스 속성 재설정 (Multiple Sprite 슬라이스 적용)
                EditorUtility.DisplayProgressBar("Sprite Packer", $"아틀라스 시트 {sheetIndex + 1} 슬라이스 설정 중...", 0.8f);
                TextureImporter atlasImporter = AssetImporter.GetAtPath(currentSavePath) as TextureImporter;
                if (atlasImporter != null)
                {
                    atlasImporter.textureType = TextureImporterType.Sprite;
                    atlasImporter.spriteImportMode = SpriteImportMode.Multiple;
                    atlasImporter.mipmapEnabled = false;
                    atlasImporter.npotScale = TextureImporterNPOTScale.None;
                    atlasImporter.maxTextureSize = maxAtlasSize;

                    SpriteRect[] newMeta = new SpriteRect[packCount];
                    for (int i = 0; i < packCount; i++)
                    {
                        Rect uv = uvs[i];
                        Rect pixelRect = new Rect(
                            Mathf.RoundToInt(uv.x * atlas.width),
                            Mathf.RoundToInt(uv.y * atlas.height),
                            Mathf.RoundToInt(uv.width * atlas.width),
                            Mathf.RoundToInt(uv.height * atlas.height)
                        );

                        SpriteMetaInfo oldMeta = remainingMetas[i];
                        newMeta[i] = new SpriteRect
                        {
                            name = oldMeta.name,
                            rect = pixelRect,
                            alignment = (SpriteAlignment)oldMeta.alignment,
                            pivot = oldMeta.pivot,
                            border = oldMeta.border,
                            spriteID = new GUID(oldMeta.spriteID) // GUID 고정
                        };
                    }

                    // 여기서 구버전 API(spritesheet) 대신 ISpriteEditorDataProvider를 사용하여 안전하게 스프라이트 정보 갱신
                    var factory = new SpriteDataProviderFactories();
                    factory.Init();
                    var dataProvider = factory.GetSpriteEditorDataProviderFromObject(atlasImporter);
                    dataProvider.InitSpriteEditorDataProvider();
                    dataProvider.SetSpriteRects(newMeta);
                    dataProvider.Apply();

                    atlasImporter.SaveAndReimport();

                    // ScriptableObject(Data 파일) 생성 및 업데이트 (인스펙터 확인 용도)
                    string dataPath = currentSavePath.Replace(".png", "_Data.asset");
                    CustomSpriteAtlasData atlasData = AssetDatabase.LoadAssetAtPath<CustomSpriteAtlasData>(dataPath);
                    if (atlasData == null)
                    {
                        atlasData = ScriptableObject.CreateInstance<CustomSpriteAtlasData>();
                        AssetDatabase.CreateAsset(atlasData, dataPath);
                    }

                    atlasData.atlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(currentSavePath);
                    atlasData.sprites.Clear();

                    foreach (var meta in newMeta)
                    {
                        atlasData.sprites.Add(new SpriteMetaInfo
                        {
                            name = meta.name,
                            rect = meta.rect,
                            alignment = (int)meta.alignment,
                            pivot = meta.pivot,
                            border = meta.border,
                            spriteID = meta.spriteID.ToString()
                        });
                    }

                    EditorUtility.SetDirty(atlasData);
                    AssetDatabase.SaveAssets();
                }

                // 메모리 해제
                Object.DestroyImmediate(atlas);

                // 패킹 완료된 텍스처 제거
                remainingTextures.RemoveRange(0, packCount);
                remainingMetas.RemoveRange(0, packCount);
                sheetIndex++;
            }

            // 5. 사용된 원본 파일 처리 (삭제 또는 복원)
            EditorUtility.DisplayProgressBar("Sprite Packer", "마무리 중...", 0.9f);
            if (deleteOriginals && successAll)
            {
                foreach (string path in originalPaths)
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
            else
            {
                foreach (string path in originalPaths)
                {
                    TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (ti != null && originalReadableStates.ContainsKey(path))
                    {
                        ti.isReadable = originalReadableStates[path];
                        ti.textureCompression = originalCompressions[path];
                        if (originalNPOTScales.ContainsKey(path))
                            ti.npotScale = originalNPOTScales[path];
                        ti.SaveAndReimport();
                    }
                }
            }

            if (successAll)
            {
                texturesToPack.Clear();
                preExtractedMeta.Clear();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("성공", $"시트 {sheetIndex}장으로 아틀라스 패킹이 완료되었습니다!", "확인");
            }
            else
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("경고", "일부 이미지가 지정된 Max Atlas Size보다 커서 패킹에 실패했습니다. (콘솔 확인)", "확인");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError($"Sprite Packing Error: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("에러", "스프라이트 패킹 도중 문제가 발생했습니다. 콘솔을 확인해주세요.", "확인");
        }
    } // This brace closes the method containing the try-catch block.

    private void ExtractSpritesFromAtlas(Texture2D atlasTex, TextureImporter importer)
    {
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();
        var spriteRects = dataProvider.GetSpriteRects();

        if (spriteRects == null || spriteRects.Length == 0) return;

        // ScriptableObject 기반 데이터가 존재한다면 메타데이터 최우선 적용 (GUID 꼬임 방지 용도)
        string dataPath = importer.assetPath.Replace(".png", "_Data.asset");
        CustomSpriteAtlasData atlasData = AssetDatabase.LoadAssetAtPath<CustomSpriteAtlasData>(dataPath);

        List<SpriteMetaInfo> metaInfoList = new List<SpriteMetaInfo>();

        for (int i = 0; i < spriteRects.Length; i++)
        {
            var sr = spriteRects[i];
            SpriteMetaInfo info = new SpriteMetaInfo
            {
                name = sr.name,
                rect = sr.rect,
                alignment = (int)sr.alignment,
                pivot = sr.pivot,
                border = sr.border,
                spriteID = sr.spriteID.ToString()
            };

            // 외부 데이터가 있다면 이름 매칭을 통해, 기존에 지정한 변경 값과 고유 GUID를 복원합니다.
            if (atlasData != null && atlasData.sprites.Count > 0)
            {
                var savedInfo = atlasData.sprites.Find(s => s.name == sr.name);
                if (savedInfo != null)
                {
                    info.alignment = savedInfo.alignment;
                    info.pivot = savedInfo.pivot;
                    info.border = savedInfo.border;
                    info.spriteID = savedInfo.spriteID; // Load preserved GUID
                }
            }
            metaInfoList.Add(info);
        }

        // 원본 아틀라스를 읽기 위해 임시로 Readable 설정
        bool wasReadable = importer.isReadable;
        TextureImporterCompression origCompress = importer.textureCompression;

        if (!wasReadable || origCompress != TextureImporterCompression.Uncompressed)
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Texture2D readableAtlas = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);

        foreach (var meta in metaInfoList)
        {
            int x = Mathf.RoundToInt(meta.rect.x);
            int y = Mathf.RoundToInt(meta.rect.y);
            int width = Mathf.RoundToInt(meta.rect.width);
            int height = Mathf.RoundToInt(meta.rect.height);

            // 잘라낼 영역 유효성 검사
            if (x < 0 || y < 0 || width <= 0 || height <= 0 || x + width > readableAtlas.width || y + height > readableAtlas.height)
                continue;

            Color[] pixels = readableAtlas.GetPixels(x, y, width, height);
            Texture2D newTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            newTex.SetPixels(pixels);
            newTex.Apply();
            newTex.name = meta.name;

            // 추출한 텍스처와 메타데이터 기억
            texturesToPack.Add(newTex);
            preExtractedMeta[newTex] = meta;
        }

        // 원상복구
        if (!wasReadable || origCompress != TextureImporterCompression.Uncompressed)
        {
            importer.isReadable = wasReadable;
            importer.textureCompression = origCompress;
            importer.SaveAndReimport();
        }

        Debug.Log($"아틀라스 '{atlasTex.name}'에서 {metaInfoList.Count}개의 스프라이트를 추출하여 목록에 추가했습니다.");
    }
}
#endif
