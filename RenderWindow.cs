using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RenderWindow : EditorWindow
{ 
    private static RenderWindow  _renderWindow;
    private PreviewRenderUtility _previewRenderer;
    
    private const float PREVIEW_AREA_HEIGHT = 450;
    private const float CAMERA_FIELD_OF_VIEW = 60f;
    private const float CAMERA_FAR_CLIP_PLANE = 1000f;

    private GameObject _objToRender;
    private float      _cameraDistance = -6f;
    private Rect       _previewArea;
    private Vector2    _lastMousePosition;
    private Vector2    _deltaMousePosition;
    private Vector3    _cameraRotation = new Vector3(45f,0.0f,0.0f);
    
    [MenuItem("Window/Render Window")]
    public static void Init()
    {
        _renderWindow = GetWindow<RenderWindow>("Render Window", true);
        _renderWindow.autoRepaintOnSceneChange = true;
        _renderWindow.Show();
    }

    private void InitRenderer()
    {
        _previewRenderer = new PreviewRenderUtility {cameraFieldOfView = CAMERA_FIELD_OF_VIEW};
        _previewRenderer.camera.backgroundColor = Color.black;
        _previewRenderer.camera.transform.position = new Vector3(0, 0.0f, -7);
        _previewRenderer.camera.farClipPlane = CAMERA_FAR_CLIP_PLANE;
    }

    private void Update()
    {
        Repaint();
    }
    
    private void OnDestroy()
    {
        _previewRenderer.Cleanup();
    }

    private void OnGUI()
    {
        if (_previewRenderer == null) InitRenderer();

        //--Obj Instance Area---------------------
        EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Object to render: ");
                _objToRender = (GameObject)EditorGUILayout.ObjectField(_objToRender,typeof(GameObject));
            EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        //---------------------------------------
        
        if (_objToRender == null)
        {
            ErrorHandler("Object to render is Null!", MessageType.Error);
            return;
        }
        
        var meshFilters = _objToRender.GetComponentsInChildren<MeshFilter>();
        var skinnedMeshRenderers = _objToRender.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        if(meshFilters.Length == 0)  ErrorHandler("Missing shared mesh", MessageType.Error);
        if(skinnedMeshRenderers.Length == 0) ErrorHandler("Missing skinned meshes",MessageType.Info);
        
        Input_ObjRotation();
        
        //--Render Area-------------------------
        GUILayout.BeginArea (new Rect (0,position.height - PREVIEW_AREA_HEIGHT, position.width, PREVIEW_AREA_HEIGHT));
             _previewArea = new Rect(0, 0, position.width, PREVIEW_AREA_HEIGHT);
            _previewRenderer.BeginPreview(_previewArea, GUIStyle.none);

            //Taking all meshes and render all structure of object
            foreach (var filter in meshFilters)
            {
                var meshRenderer = filter.GetComponent<MeshRenderer>();
                if (meshRenderer)
                {
                    DrawMesh(filter.sharedMesh, meshRenderer.sharedMaterial, filter.gameObject.transform);
                }
            }
            
            //Taking all skins and render all structure of object
            foreach (var skin in skinnedMeshRenderers)
            {
                var mesh = new Mesh();
                skin.BakeMesh(mesh);
                DrawMesh(mesh, skin.sharedMaterial, skin.gameObject.transform);
            }
            
            _previewRenderer.camera.Render();
            var render = _previewRenderer.EndPreview();
            GUI.DrawTexture(new Rect(0, 0, _previewArea.width, _previewArea.height), render);
            
        GUILayout.EndArea ();
        //-------------------------------------
    }
    
    private void DrawMesh(Mesh mesh, Material material, Transform transform)
    {
        _previewRenderer.DrawMesh(mesh, Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale), material, 0);
    }
    
    private void Input_ObjRotation()
    {
        Debug.Log(_cameraDistance);
        
        //Get wheel value and set up camera distance
        if( Event.current.type == EventType.ScrollWheel )
        {
            _cameraDistance += Event.current.delta.y;
            _previewRenderer.camera.transform.position = new Vector3(0 ,0,_cameraDistance);
        }

        //Get mouseDown event and Mouse drag for rotation of renderObj
        switch( Event.current.type )
        {
            case EventType.MouseDown:
                _lastMousePosition =  GUIUtility.GUIToScreenPoint( Event.current.mousePosition );
                break;
            case EventType.MouseDrag:
            {
                //Calculation if mouse is in RenderArea
                var mousePos = GUIUtility.GUIToScreenPoint( Event.current.mousePosition );
                var isRenderArea = IsMouseInRenderArea(mousePos);

                //Calculate amount of rotation
                _deltaMousePosition = mousePos - _lastMousePosition;
                if (isRenderArea)
                {
                    _objToRender.transform.Rotate(0, _deltaMousePosition.x * -1, 0);
                    _cameraRotation = new Vector3(_cameraRotation.x + _deltaMousePosition.y,_cameraRotation.y,_cameraRotation.z);
                }

                _lastMousePosition = mousePos;
                break;
            }
        }
        
        //Calculate orbit rotation 
        var renderObjPosition = _objToRender.transform.position;
        var xClamp = Mathf.Clamp(_cameraRotation.x, 0f, 90f);
        _previewRenderer.camera.transform.position = renderObjPosition + Quaternion.Euler( xClamp, 0, _cameraRotation.z ) * ( _cameraDistance * Vector3.forward );
        _previewRenderer.camera.transform.LookAt(worldPosition: renderObjPosition, Vector3.up);
    }

    private bool IsMouseInRenderArea(Vector2 mousePos)
    {
        //TODO: Fix mouse Renderer
        //Note: Editor has origin top, left (0,0)
        //Note: window position is worldPosition and getting position window depends,
        //on where in editor is so its reason why I need calculate right site or bottom site with plus window position
        var isMouseInWindowX = mousePos.x > position.x && mousePos.x < (position.width + position.x); 
        var isMouseInWindowY = mousePos.y > position.y && mousePos.y < (position.height + position.y);

        //Width of render area is for all width of window so calculate classic method if mouse is in window
        var isMouseInRenderAreaX = mousePos.x > position.x && mousePos.x < position.width;
            
        //Height of render area is not for all window so I need use PREVIEW_AREA_HEIGHT for get exactly 
        //(position.height - _previewArea.height) -> we need calculate difference of this because,
        //we want not render area from window and this value calculate with position of window
        var isMouseInRenderAreaY = mousePos.y > (position.y + (position.height - _previewArea.height)) && mousePos.y < (position.height + position.y);
        
        return isMouseInRenderAreaX && isMouseInRenderAreaY;
    }

    private void ErrorHandler(string msg, MessageType type)
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.HelpBox(msg, type );
        EditorGUILayout.EndVertical();
        Debug.Log("RenderWindow-> " + msg);
    }
    
}
