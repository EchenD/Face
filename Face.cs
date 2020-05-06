///<summary>
/// Main Face Rig Controller
/// </summary>
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class Face : Singleton<Face>
{
    protected Face() { }

    public List<Helper> _activeHelpers;//SelectedHelpers
    public Helper InterActiveHelper;
    public _HelperSelections _activeHelperSelection;//Selected HelperSelection
    public _eyeHelperSelection _activeEyeSelection;

    public GameObject _faceModel; //Model GameObject
    public GameObject Skull; //Skull
    public GameObject _ControllerHelperPref;// Control Prefab For points
    public GameObject _NormalPrefab;// Prefab for showing Normal
    public GameObject _SelectionHelperPref;// Prefab for showing Normal
    public GameObject _MorphControllerPrefab;// MorphController UI Prefab
    public GameObject _AxisPrefab;//Prefab for Showing Axis

    public Transform _MeshHead;//Mesh Head
    public Transform[] _eyes, _ears, _eyebrows, _cP, _cS, _cT;//Selection Points,Points 
    public Transform _lips, _nose;//Selection Points
    public List<Helper> _Helpers = new List<Helper>();//List of Points
    public List<_HelperSelections> _HelpersSelection = new List<_HelperSelections>();//List of Selection Point
    public List<HelperPositionConstraint> _HelperPositionConstraints = new List<HelperPositionConstraint>();//List of Position Constraints
    public _eyeHelperSelection[] _eyeHelperSelections = new _eyeHelperSelection[2];//EyeHelperSelections
    public Gradient[] _Grads;//Color Gradients
    public Material[] _materials;//face Materials
    public Helper[][] _lvl = new Helper[10][];//Points Level
    public Dictionary<string, Transform> _boneRotation = new Dictionary<string, Transform>();//dictionary of bones and thair post eulerAngles

    public float _ControllerInfluence = 1;//Corrent Controller Influence
    public int MaskLvl = 10;//Maske Level

    public SkinnedMeshRenderer _skinnedMeshRenderer;//Head skinMesh Renderer
    public Mesh _skinnedMesh;//Head SkinMesh Renderer Mesh

    public List<_morpherController> _MorphAll = new List<_morpherController>();// All the morph Controllers

    public Renderer _renderer;//Head Renderer
    public Renderer[] _eyeRenderer = new Renderer[2];//Eyes Renderers
    public Renderer _eyeLashesRenderer;//Eyelasehes Renderer
    public SkinnedMeshRenderer _eyeLashesSkinnedMeshRenderer;//EyeLashes skinMeshRenderer
    public bool _symmetry = true;//If symmetry is of or off
    public Vector3 MasterPosition;
    public Quaternion MasterRotation;
    public float MasterScale;
    public bool movement = true, rotation = false, scale = false;
    public bool InputFieldActive;//if any inputField is Active 
    public int _i = 3;//previews selected Helpers Id
    public MeshCollider _mc;//Face Mesh Collider
    public GameObject Scan;//Loaded Scan
    public Vector3[] DefaultHeadVerts;//Storing Default Mesh Vertesies
    public Vector3[] DefaultHeadNorms;//Storing Default Mesh Vertesies
    public Dictionary<int, int[]> CuttedVertesiesAroundUVEdges;// List of New Vertesies that is created during import from 3d pakages

    private bool _WireFrame;//If shows WireFrame
    private bool _RefImages;//If show Reference Images
    private bool _multiSelection = false;//If Theres is Point MultiSelection
    private int _blendShapeCount;//Head Blendshape Count
    private List<_morpherController>
        _MCEyes = new List<_morpherController>(),
        _MCNose = new List<_morpherController>(),
        _MCLips = new List<_morpherController>(),
        _MCEars = new List<_morpherController>(),
        _MCSkull = new List<_morpherController>(),
        _MCFat = new List<_morpherController>(),
        _MCAge = new List<_morpherController>(),
        _MCSex = new List<_morpherController>(),
        _MCRace = new List<_morpherController>(),
        _MCMist = new List<_morpherController>();//List of MorphControllers
    private List<Helper[]> _lvlfound = new List<Helper[]>();//List of Level of Found Helpers 
    private _AxisController _AxisController;//Created Axis Controller
    private _morpherController[] _selectedMorphGroup = new _morpherController[0];//Current Selected MorphGroups
    private bool _out = false;//if mouse position is out of face
    private Material[] WireFrameMats = new Material[2];//Selected WireFrame Mode Materials
    private bool WireFrameSolid = true;//if Wire Frame Mode is Solid or Transparent

   
    // Apply Position Constraint and Symmetry
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SetWireFrame();//WireFrame on and off
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            _setWireFrameSolid();//WireFrame Solid Transparent
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ShowReferenceImages();//Reference Image on and off
        }
        if (Input.GetKeyDown(KeyCode.M) && !InputFieldActive)
        {
            MakeSymmetry();//Make the point Symmetrycal
        }
        if (Input.GetKeyDown(KeyCode.S) && !Input.GetKey(KeyCode.LeftControl) && !InputFieldActive)// Turn On and off Symmetry
        {
            if (_activeHelperSelection == null && _activeEyeSelection == null && _activeHelpers.Count < 1)
            {
                _symmetry = !_symmetry;
                UI.Instance._images[0].color = _symmetry ? UI.Instance._colors[1] : UI.Instance._colors[0];
            }
            else
            {
                SetSymmetry();
            }
        }
        if ((_activeHelperSelection != null || _activeEyeSelection != null) && !InputFieldActive)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                SetTransform(0);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                SetTransform(1);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1) && !InputFieldActive)
        {
            ShowPoints(0);//show Selection Points
            removeAllActiveHelper();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && !InputFieldActive)
        {
            ShowPoints(1);//show OpenFace User Poins
            removeAllActiveHelper();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && !InputFieldActive)
        {
            ShowPoints(2);//Shpw All the Points
            removeAllActiveHelper();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && !InputFieldActive)
        {
            ShowPoints(3);//hide all the Points
            removeAllActiveHelper();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _multiSelection = true; //MultiSelection is on
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            _multiSelection = false; //MultiSelection is off
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShowHideScan();
        }
        Vector3 delta = new Vector3();
        switch (_i)
        {
            case 0:
                if (_activeHelperSelection == null)
                    return;
                if (!checkActiveHelperSelection())
                    return;
                if (_activeHelperSelection.OnAxis)
                    if (movement)
                    {
                        delta = MasterPosition - _activeHelperSelection.OldPosition;
                        _activeHelperSelection.Master.position += delta;
                        _activeHelperSelection.OldPosition = _activeHelperSelection.Master.position;
                        if (_symmetry)
                        {
                            if (!_activeHelperSelection.SymmetryFreeze)
                            {
                                _activeHelperSelection._HSSymmetry.Master.position += new Vector3(-delta.x, delta.y, delta.z);
                                _activeHelperSelection._HSSymmetry.OldPosition = _activeHelperSelection._HSSymmetry.Master.position;

                            }
                        }
                    }
                    else if (rotation)
                    {
                        _activeHelperSelection.Master.localRotation = MasterRotation;
                        _activeHelperSelection.OldRotation = _activeHelperSelection.Master.localRotation;
                        if (_symmetry)
                        {
                            if (!_activeHelperSelection.SymmetryFreeze)
                            {
                                _activeHelperSelection._HSSymmetry.Master.localEulerAngles =
                                    new Vector3(_activeHelperSelection.Master.localEulerAngles.x,
                                    -_activeHelperSelection.Master.localEulerAngles.y,
                                    -_activeHelperSelection.Master.localEulerAngles.z);
                                _activeHelperSelection._HSSymmetry.OldRotation = _activeHelperSelection._HSSymmetry.Master.localRotation;

                            }
                        }
                    }
                break;
            case 1:
            case 2:
                if (_activeEyeSelection != null)
                {
                    if (checkEyeSelection())
                    {
                        if (_activeEyeSelection.OnAxis)
                            if (movement)
                            {
                                delta = MasterPosition - _activeEyeSelection.OldPosition;
                                _activeEyeSelection.transform.position += delta;
                                _activeEyeSelection.OldPosition = _activeEyeSelection.transform.localPosition;
                                if (_symmetry)
                                {
                                    _activeEyeSelection.SymEye.transform.localPosition += new Vector3(-delta.x, delta.y, delta.z);
                                    _activeEyeSelection.SymEye.OldPosition = _activeEyeSelection.SymEye.transform.localPosition;
                                }
                            }
                            else if (rotation)
                            {
                                _activeEyeSelection.transform.localRotation = MasterRotation;
                                _activeEyeSelection.OldRotation = _activeEyeSelection.transform.localRotation;
                                if (_symmetry)
                                {
                                    _activeEyeSelection.SymEye.transform.localEulerAngles = new Vector3(
                                        _activeEyeSelection.transform.localEulerAngles.x,
                                        -_activeEyeSelection.transform.localEulerAngles.y,
                                        -_activeEyeSelection.transform.localEulerAngles.z);
                                    _activeEyeSelection.SymEye.OldRotation = _activeEyeSelection.SymEye.transform.localRotation;
                                }
                            }
                    }
                }
                if (_activeHelpers.Count < 1)
                    return;
                if (!checkActiveHelper())//check if Active Helpers are available
                    return;
                if (InterActiveHelper.OnAxis)
                {
                    delta = MasterPosition - InterActiveHelper.OldPosition;
                    foreach (Helper _hr in _activeHelpers)
                    {
                        _hr.Master.position += delta;
                        _hr.OldPosition = _hr.Master.position;
                    }
                    if (_symmetry)
                    {
                        foreach (Helper _hr in _activeHelpers)
                            if (!_hr.SymmetryFreeze)
                            {
                                _hr.SymmetryHelper.position += new Vector3(-delta.x, delta.y, delta.z);
                                _hr.hSymmetryHelper.OldPosition = _hr.SymmetryHelper.position;
                            }
                    }
                }
                break;
        }
    }

    public void BakeMeshForCollider()
    {
        Mesh m = new Mesh();
        _skinnedMeshRenderer.BakeMesh(m);
        _mc.sharedMesh = m;

    }

    public void ResetDeltaMeshDeformation()
    {
        _skinnedMesh.vertices = DefaultHeadVerts;
        _skinnedMesh.normals = DefaultHeadNorms;
    }

    void FixedUpdate()
    {

        if (PreviewPanel.IsOverPreviewPanel)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                MoveCamera.Instance.CameraFaceDistance = hit.distance;
                if (_i == 2 || _i == 1)
                {
                    if (!_out)
                        _out = true;
                    foreach (Helper _hr in _Helpers)
                    {
                        Color _co = new Color();
                        float distance = Vector3.Distance(hit.point, _hr.transform.position);
                        if (distance < .05f)
                        {
                            _co = _hr._mr.material.color;
                            _hr._mr.material.color = new Color(_co.r, _co.g, _co.b, (1 - (distance / .05f)));
                            if (_hr.Child.gameObject.layer != 10)
                            {
                                _hr.Child.gameObject.layer = 10;
                                _hr.gameObject.layer = 10;
                            }
                        }
                        else
                        {
                            if (_hr.Child.gameObject.layer != 13)
                            {
                                _hr.Child.gameObject.layer = 13;
                                _hr.gameObject.layer = 13;
                            }
                        }
                    }
                }
            }
            else
            {
                if (_out)
                {
                    _out = false;
                    foreach (Helper _hr in _Helpers)
                    {
                        if (_hr.Child.gameObject.layer != 13)
                        {
                            _hr.Child.gameObject.layer = 13;
                            _hr.gameObject.layer = 13;
                        }
                    }
                }

            }
        }
    }

    void LateUpdate()
    {
        foreach (HelperPositionConstraint hl in _HelperPositionConstraints)
        {
            CalculateConstraintPositions(hl);
        }
    }

    void CalculateConstraintPositions(HelperPositionConstraint hl)
    {
        for (int i = 0; i < hl.Neighbors.Count; i++)
        {

            hl.oldPosition[i] = hl.oldPosition[i] == Vector3.zero ? hl.Neighbors[i].position : hl.oldPosition[i];
            hl.newPosition[i] = hl.Neighbors[i].position;
            hl.deltaPosition[i] = (hl.newPosition[i] - hl.oldPosition[i]);
            hl.transform.position += hl.deltaPosition[i] * (hl.Weight[hl.Neighbors[i]]) * hl.OverallWeight;
            hl.oldPosition[i] = hl.newPosition[i];
        }
    }


    //check if Helpers is selected and available
    bool checkActiveHelper()
    {
        if (!InterActiveHelper.MouseOnObject && Input.GetMouseButtonDown(0) && !InterActiveHelper.OnAxis && PreviewPanel.IsOverPreviewPanel && !_multiSelection)
        {
            removeAllActiveHelper();
            if (_activeEyeSelection == null)
                UI.Instance.EditPanelShow(0);
            return false;
        }
        return true;
    }

    bool checkActiveHelperSelection()
    {
        if (!_activeHelperSelection.MouseOnObject && Input.GetMouseButtonDown(0) && !_activeHelperSelection.OnAxis && PreviewPanel.IsOverPreviewPanel)
        {
            _activeHelperSelection._Unselect();
            UI.Instance.EditPanelShow(0);
            return false;
        }
        return true;
    }

    bool checkEyeSelection()
    {
        if (!_activeEyeSelection.MouseOnObject && Input.GetMouseButtonDown(0) && !_activeEyeSelection.OnAxis && PreviewPanel.IsOverPreviewPanel)
        {
            _activeEyeSelection._Unselect();
            if (_activeHelpers.Count < 1)
                UI.Instance.EditPanelShow(0);
            return false;
        }
        return true;
    }

    public void SetSelectionScale()
    {
        if (_activeHelperSelection == null)
            return;
        _activeHelperSelection.Master.localScale = new Vector3(MasterScale, MasterScale, MasterScale);
        _activeHelperSelection.OldScale = _activeHelperSelection.Master.localScale;
        if (_symmetry)
        {
            if (!_activeHelperSelection.SymmetryFreeze)
            {
                _activeHelperSelection._HSSymmetry.Master.localScale = new Vector3(MasterScale, MasterScale, MasterScale);
                _activeHelperSelection._HSSymmetry.OldScale = _activeHelperSelection._HSSymmetry.Master.localScale;

            }
        }
    }
    public void SetEyeScale()
    {
        if (_activeEyeSelection == null)
            return;
        _activeEyeSelection.transform.localScale = new Vector3(MasterScale, MasterScale, MasterScale);
        _activeEyeSelection.OldScale = _activeEyeSelection.transform.localScale;
        if (_symmetry)
        {
            _activeEyeSelection.SymEye.transform.localScale = new Vector3(MasterScale, MasterScale, MasterScale);
            _activeEyeSelection.SymEye.OldScale = _activeEyeSelection.SymEye.transform.localScale;
        }
    }
    //Remove Selected Helper
    public void removeAllActiveHelper()
    {
        foreach (Helper _hr in _activeHelpers)
            _helperUnSelected(_hr);
        _activeHelpers.Clear();
        InterActiveHelper = null;
        if (_activeEyeSelection == null)
            SetAxies(3, "");//Hide Axis
    }

    //Check Rig Type
    RigType chckRigType()
    {
        RigType RT = RigType.Generic;
        if (_faceModel.name.Contains("Head"))
            RT = RigType.head;
        else if (_faceModel.name.Contains("Body"))
            RT = RigType.Body;
        UI.WriteToConsole("Loading RigType: " + RT.ToString());
        return RT;
    }
    // Reading Imported Rig
    public void Initialize()
    {
        RigType RT = chckRigType();
        switch (RT)
        {
            case RigType.head:
                _eyes = new Transform[2];
                _ears = new Transform[2];
                _eyebrows = new Transform[2];

                Transform FacialFutureSelection = _faceModel.transform.GetChild(0);
                Transform HighPoliesMeshes = _faceModel.transform.GetChild(1);
                Transform PointParent = _faceModel.transform.GetChild(2);
                int Pcount = 0;
                int Scount = 0;
                int Tcount = 0;

                for (int i = 0; i < FacialFutureSelection.childCount; i++)
                {
                    Pcount += FacialFutureSelection.GetChild(i).childCount;
                }
                Pcount -= 2;//for Eye Meshes
                for (int i = 0; i < PointParent.childCount; i++)
                {
                    if (PointParent.GetChild(i).name == "Primary")
                        Pcount += PointParent.GetChild(i).childCount;
                    if (PointParent.GetChild(i).name == "Secondary")
                        Scount += PointParent.GetChild(i).childCount;
                    if (PointParent.GetChild(i).name == "Tertiary")
                        Tcount += PointParent.GetChild(i).childCount;
                }
                _cP = new Transform[Pcount];
                _cS = new Transform[Scount];
                _cT = new Transform[Tcount];
                for (int i = 0; i < FacialFutureSelection.childCount; i++)
                {
                    Transform _obj = FacialFutureSelection.GetChild(i);
                    string _name = FacialFutureSelection.GetChild(i).name;
                    _HelperSelections _selecCG = CreateSelectionHelper(_obj, _name);
                    _HelpersSelection.Add(_selecCG);
                    for (int j = 0; j < _obj.childCount; j++)
                    {
                        if (_obj.GetChild(j).name[0] == 'P')
                        {
                            CreateHelper(_obj.GetChild(j), _obj.GetChild(j).name, 0);
                            _selecCG._Points.Add(_obj.GetChild(j));
                        }
                        else if (_obj.GetChild(j).name == "eye_l")
                        {
                            _obj.GetChild(j).gameObject.layer = 12;
                            _eyeHelperSelections[0] = _obj.GetChild(j).gameObject.AddComponent<_eyeHelperSelection>();
                            _eyeHelperSelections[0]._collider = _eyeHelperSelections[0].gameObject.AddComponent<SphereCollider>();
                            _eyeRenderer[0] = _obj.GetChild(j).GetComponent<Renderer>();
                        }
                        else if (_obj.GetChild(j).name == "eye_r")
                        {
                            _obj.GetChild(j).gameObject.layer = 12;
                            _eyeHelperSelections[1] = _obj.GetChild(j).gameObject.AddComponent<_eyeHelperSelection>();
                            _eyeHelperSelections[1]._collider = _eyeHelperSelections[1].gameObject.AddComponent<SphereCollider>();
                            _eyeRenderer[1] = _obj.GetChild(j).GetComponent<Renderer>();
                        }
                    }
                    string _fst = _name.Split('_')[0];
                    switch (_fst)
                    {
                        case "eye": if (_name.Contains("_r")) _eyes[0] = _obj; else _eyes[1] = _obj; break;
                        case "ear": if (_name.Contains("_r")) _ears[0] = _obj; else _ears[1] = _obj; break;
                        case "eyebrows": if (_name.Contains("_r")) _eyebrows[0] = _obj; else _eyebrows[1] = _obj; break;
                        case "nose": _nose = _obj; break;
                        case "lips": _lips = _obj; break;
                    }
                }
                for (int i = 0; i < PointParent.childCount; i++)
                {
                    for (int j = 0; j < PointParent.GetChild(i).childCount; j++)
                    {
                        if (PointParent.GetChild(i).name == "Primary")
                            CreateHelper(PointParent.GetChild(i).GetChild(j), PointParent.GetChild(i).GetChild(j).name, 0);
                        if (PointParent.GetChild(i).name == "Secondary")
                            CreateHelper(PointParent.GetChild(i).GetChild(j), PointParent.GetChild(i).GetChild(j).name, 1);
                        if (PointParent.GetChild(i).name == "Tertiary")
                            CreateHelper(PointParent.GetChild(i).GetChild(j), PointParent.GetChild(i).GetChild(j).name, 2);
                    }
                }
                for (int i = 0; i < HighPoliesMeshes.childCount; i++)
                {
                    Transform _obj = HighPoliesMeshes.GetChild(i);
                    string _name = HighPoliesMeshes.GetChild(i).name;
                    if (_name.Contains("Head"))
                    {
                        _MeshHead = _obj;
                        _renderer = _MeshHead.GetComponent<Renderer>();
                        _skinnedMeshRenderer = _MeshHead.GetComponent<SkinnedMeshRenderer>();
                        _skinnedMesh = _MeshHead.GetComponent<SkinnedMeshRenderer>().sharedMesh;
                        _mc = _MeshHead.gameObject.AddComponent<MeshCollider>();
                        _mc.sharedMesh = _skinnedMesh;
                    }
                    else if (_name.Contains("Eyelahses"))
                    {
                        _obj.gameObject.layer = 17;
                        _eyeLashesRenderer = _obj.GetComponent<Renderer>();
                        _eyeLashesSkinnedMeshRenderer = _obj.GetComponent<SkinnedMeshRenderer>();
                    }
                }
                UI.WriteToConsole("Setting Helpers");
                _loading.setLoading(.5f, false);
                setNeibours();
                foreach (Helper _helper in _Helpers)
                {
                    _helper.setConnections();
                }
                UI.WriteToConsole("Rig Successfully Loaded", UI.Instance._colors[1]);
                CreateBlendShapes();
                _loading.setLoading(0, false);

                Material[] mat = _renderer.materials;
                StandardShaderUtils.ChangeRenderMode(mat[0], BlendMode.Opaque);
                StandardShaderUtils.ChangeRenderMode(mat[1], BlendMode.Opaque);
                _materials[0] = mat[0];
                _materials[1] = mat[1];
                _AxisController = Instantiate(_AxisPrefab).GetComponent<_AxisController>();
                SetAxies(3, "");//Hide Axis
                foreach (_HelperSelections _hs in _HelpersSelection) _hs.hide(false);
                foreach (Helper _h in _Helpers) { _h.showHide(false); };
                foreach (_eyeHelperSelection _he in _eyeHelperSelections) _he.DeActive();
                WireFrameMats[0] = _materials[2];
                WireFrameMats[1] = _materials[3];
                break;
            case RigType.Body:
                //Load Rig Body
                break;
            case RigType.Generic:
                //Load Generic Rig
                break;
        }
    }

    //create Point Helpers
    void CreateHelper(Transform _obj, string _name, int state)
    {
        GameObject _Helper = Instantiate(_ControllerHelperPref, _obj, true);
        _Helper.transform.position = _obj.transform.position;
        _Helper.name = _name.Split('_')[0] + "_GCP";
        char[] trimchar = { 'P', 'S', 'T' };
        int index = int.Parse(_name.Split('_')[0].TrimStart(trimchar));
        Helper _helper = _Helper.GetComponent<Helper>();
        _helper.Master = _obj;
        IntHelper(_helper);
        if (state == 0)
        {
            _cP[index] = _obj;
            _helper._type = FaceControllerType.OpenFace;
            foreach (_bone _bone in _helper._Bones) { _bone._PBoneCons = true; }
        }
        else if (state == 1)
        {
            _cS[index] = _obj;
            _helper._type = FaceControllerType.User;
            foreach (_bone _bone in _helper._Bones) { _bone._SBoneCons = true; }
        }
        else if (state == 2)
        {
            _cT[index] = _obj;
            _helper._type = FaceControllerType.Helper;
        }
        _helper.setColor();
        _Helpers.Add(_helper);
    }


    //create selection Helper
    _HelperSelections CreateSelectionHelper(Transform _obj, string _name)
    {
        GameObject _selectionHelper = Instantiate(_SelectionHelperPref, _obj, true);
        _selectionHelper.transform.position = _obj.transform.position;
        _selectionHelper.name = _name.Split('_')[0] + "_SCP";
        _HelperSelections _helperS = _selectionHelper.GetComponent<_HelperSelections>();
        _helperS.Master = _obj;
        return (_helperS);
    }

    // Initializes Helper creating bones
    void IntHelper(Helper _helper)
    {
        if (_helper.Master.transform.childCount == 0)
            return;
        for (int i = 0; i < _helper.Master.transform.childCount; i++)
        {
            if (_helper.Master.transform.GetChild(i).name.Contains("_GCP"))
                continue;
            GameObject obj = _helper.Master.transform.GetChild(i).GetChild(0).GetChild(0).gameObject;
            _bone bone = obj.AddComponent<_bone>();
            //Instantiate(_NormalPrefab,bone.transform,false);
            bone.CreateBone();
            _helper._Bones.Add(bone);
            _boneRotation.Add(obj.name, obj.transform);
        }
    }

    //Fineds Neibours
    void setNeibours()
    {
        foreach (Helper _helper in _Helpers)
        {
            string[] _words = _helper.Master.name.Split('_')[1].Split(',');
            foreach (string st in _words)
            {
                switch (st[0])
                {
                    case 'T':
                        _helper._neighbors.Add(_cT[int.Parse(st.TrimStart('T'))]);
                        break;
                    case 'S':
                        _helper._neighbors.Add(_cS[int.Parse(st.TrimStart('S'))]);
                        break;
                    case 'P':
                        _helper._neighbors.Add(_cP[int.Parse(st.TrimStart('P'))]);
                        break;
                }
            }
            foreach (_bone _b in _helper._Bones)
            {
                string st = _b._endBone.name.Split('_')[3];
                switch (st[0])
                {
                    case 'T':
                        _b.endBoneMaster = _cT[int.Parse(st.TrimStart('T'))];
                        break;
                    case 'S':
                        _b.endBoneMaster = _cS[int.Parse(st.TrimStart('S'))];
                        _b._SBoneCons = true;
                        break;
                    case 'P':
                        _b.endBoneMaster = _cP[int.Parse(st.TrimStart('P'))];
                        _b._PBoneCons = true;
                        break;
                }
            }
            string[] Split = _helper.Master.name.Split('_');
            if (Split[2] != "c")
            {
                switch (Split[2][0])
                {
                    case 'T':
                        _helper.SymmetryHelper = _cT[int.Parse(Split[2].TrimStart('T'))];
                        break;
                    case 'S':
                        _helper.SymmetryHelper = _cS[int.Parse(Split[2].TrimStart('S'))];
                        break;
                    case 'P':
                        _helper.SymmetryHelper = _cP[int.Parse(Split[2].TrimStart('P'))];
                        break;
                }
            }
            else
            {
                _helper.SymmetryFreeze = true;
            }

        }
        foreach (Helper _helper in _Helpers)
        {
            if (_helper._type != FaceControllerType.OpenFace)
            {
                HelperPositionConstraint _pc = _helper.Master.gameObject.AddComponent<HelperPositionConstraint>();
                _HelperPositionConstraints.Add(_pc);
                foreach (Transform _tr in _helper._neighbors)
                {
                    _pc.Neighbors.Add(_tr);
                }
                _pc._Bones = _helper._Bones;
                _helper._PosConst = _pc;
            }
            if (_helper.SymmetryHelper != null)
            {
                _helper.hSymmetryHelper = _helper.SymmetryHelper.GetComponentInChildren<Helper>();
            }
        }
        foreach (_HelperSelections _hs in _HelpersSelection)
        {
            string[] Words = _hs.Master.name.Split('_');
            switch (Words[0])
            {
                case "eye":
                    if (Words[1] == "r")
                        _hs._HSSymmetry = _eyes[1].GetComponentInChildren<_HelperSelections>();
                    else
                        _hs._HSSymmetry = _eyes[0].GetComponentInChildren<_HelperSelections>();
                    break;
                case "nose":
                    _hs.SymmetryFreeze = true;
                    break;
                case "ear":
                    if (Words[1] == "r")
                        _hs._HSSymmetry = _ears[1].GetComponentInChildren<_HelperSelections>();
                    else
                        _hs._HSSymmetry = _ears[0].GetComponentInChildren<_HelperSelections>();
                    break;
                case "lips":
                    _hs.SymmetryFreeze = true;
                    break;
                case "eyebrows":
                    if (Words[1] == "r")
                        _hs._HSSymmetry = _eyebrows[1].GetComponentInChildren<_HelperSelections>();
                    else
                        _hs._HSSymmetry = _eyebrows[0].GetComponentInChildren<_HelperSelections>();
                    break;
            }
        }
        _eyeHelperSelections[0].SymEye = _eyeHelperSelections[1];
        _eyeHelperSelections[1].SymEye = _eyeHelperSelections[0];
    }

    //show wireFrame
    public void SetWireFrame()
    {
        _WireFrame = !_WireFrame;
        _setWireFrame(_WireFrame);
    }
    public void SetWireFrame(int i)
    {
        switch (i)
        {
            case 0:
                _setWireFrame(true);
                _WireFrame = true;
                break;
            case 1:
                _setWireFrame(false);
                _WireFrame = false;
                break;
        }
    }

    //Show WireFrame Solid or Transparnet
    public void _setWireFrameSolid()
    {
        if (!_WireFrame)
            return;
        Material[] mat = _renderer.materials;
        WireFrameSolid = !WireFrameSolid;
        if (!WireFrameSolid)
        {
            WireFrameMats[0] = _materials[7];
            WireFrameMats[1] = _materials[8];
        }
        else
        {
            WireFrameMats[0] = _materials[2];
            WireFrameMats[1] = _materials[3];
        }
        mat[0] = WireFrameMats[0];
        mat[1] = WireFrameMats[1];
        _renderer.materials = mat;
    }

    //Set WireFrame
    void _setWireFrame(bool State)
    {
        Material[] mat = _renderer.materials;
        if (State)
        {

            mat[0] = WireFrameMats[0];
            mat[1] = WireFrameMats[1];
            _renderer.materials = mat;
            _materials[6].color = new Color(_materials[6].color.r, _materials[6].color.g, _materials[6].color.b, 0);

        }
        else
        {
            if (_RefImages)
            {
                mat[0] = _materials[4];
                mat[1] = _materials[5];
                _renderer.materials = mat;
                _materials[6].color = new Color(_materials[6].color.r, _materials[6].color.g, _materials[6].color.b, 0);
            }
            else
            {
                mat[0] = _materials[0];
                mat[1] = _materials[1];
                _renderer.materials = mat;
                _materials[6].color = _Grads[5].Evaluate(UI.Instance._sliders[11].value);
            }
        }
        _eyeLashesRenderer.material = _materials[6];
    }
    //Show Reference Images 
    public void ShowReferenceImages(int i)
    {
        if (i == 0)//off
        {
            _RefImages = true;
            ShowReferenceImages();
        }
        else
        {
            _RefImages = false;
            ShowReferenceImages();
        }
    }
    //Show Reference Images 
    public void ShowReferenceImages()
    {
        Material[] mat = _renderer.materials;
        _RefImages = !_RefImages;
        if (_RefImages)
        {
            _setWireFrame(_WireFrame);
            for (int i = 0; i < Manager.Instance.Points.Count; i++)
                Manager.Instance.Points[i].GetChild(0).gameObject.layer = 10;
            _eyeHelperSelections[0]._Hide(false);
            _eyeHelperSelections[1]._Hide(false);
            if (Scene.Instance.ProjectorState)
            {
                Scene.Instance.SetProjector(false);
                Scene.Instance.ProjectorState = true;
            }
        }
        else
        {
            _setWireFrame(_WireFrame);
            for (int i = 0; i < Manager.Instance.Points.Count; i++)
                Manager.Instance.Points[i].GetChild(0).gameObject.layer = 0;
            _eyeHelperSelections[0]._Hide(true);
            _eyeHelperSelections[1]._Hide(true);
            if (Scene.Instance.ProjectorState)
                Scene.Instance.SetProjector(true);
        }
    }



    //Show Helpers 
    public void ShowPoints(int i)
    {
        _i = i;
        _showPoints(i);
    }
    //Show Helpers 
    void _showPoints(int i)
    {
        resetSelection();
        switch (i)
        {
            case 0://Show Selections Helper
                foreach (_HelperSelections _hs in _HelpersSelection) _hs.hide(true);
                SetTransform(0);
                break;
            case 1://show Open Face and User Helper
                foreach (Helper _h in _Helpers)
                {
                    if (_h._type != FaceControllerType.Helper)
                        _h.showHide(true);
                }
                foreach (_eyeHelperSelection _he in _eyeHelperSelections) _he.Active();
                SetTransform(0);
                break;
            case 2://show Open all Helper
                foreach (Helper _h in _Helpers) _h.showHide(true);
                foreach (_eyeHelperSelection _he in _eyeHelperSelections) _he.Active();
                SetTransform(0);
                break;
        }
    }

    //Reset Selection
    public void resetSelection()
    {
        UI.Instance.EditPanelShow(0);
        if (_activeHelpers.Count > 0)
            removeAllActiveHelper();
        if (_activeHelperSelection != null)
            _activeHelperSelection = null;
        if (_activeEyeSelection != null)
            _activeEyeSelection = null;
        foreach (_HelperSelections _hs in _HelpersSelection) _hs.hide(false);
        foreach (Helper _hl in _Helpers) _hl.showHide(false);
        foreach (_eyeHelperSelection _he in _eyeHelperSelections) _he.DeActive();
    }
    public void _selectionHelperSelected()
    {
        if (_activeHelperSelection != null)
            _activeHelperSelection._Unselect();
        UI.Instance.EditPanelShow(2);
    }


    //Helper Was selected
    public void _helperSelected(Helper _hr)
    {
        if (!_multiSelection)
        {
            removeAllActiveHelper();
        }
        UI.Instance.EditPanelShow(1);
        _activeHelpers.Add(_hr);
        findLayer();
        setInfluence(0);
        setInfluence(_ControllerInfluence);
        _CheclInterSections();
        SetWeightInfluence(_ControllerInfluence);
        _hr.activate = true;
        if (_hr._type != FaceControllerType.OpenFace)
        {
            _hr._PosConst.OverallWeight = 0;
        }
        if (_symmetry && !_hr.SymmetryFreeze)
        {
            if (_hr.hSymmetryHelper._type != FaceControllerType.OpenFace)
            {
                _hr.hSymmetryHelper._PosConst.OverallWeight = 0;
                _hr.hSymmetryHelper.activate = true;
            }
        }
    }
    //Helper Was Unselected
    public void _helperUnSelected(Helper _hr)
    {
        setInfluence(0);
        UnSetWeightInfluence(_hr);
        _hr._Unselect();
        _hr.activate = false;
        if (_hr._type != FaceControllerType.OpenFace)
        {
            _hr._PosConst.OverallWeight = 1;
        }
        if (_symmetry && !_hr.SymmetryFreeze)
        {
            UnSetWeightInfluence(_hr.hSymmetryHelper);
            _hr.hSymmetryHelper._Unselect();
            if (_hr.hSymmetryHelper._type != FaceControllerType.OpenFace)
            {
                _hr.hSymmetryHelper._PosConst.OverallWeight = 1;
                _hr.hSymmetryHelper.activate = false;
            }
        }
    }
    //Deselect selected Point
    public void DeselectSelecteHelper(Helper _hr)
    {
        _activeHelpers.Remove(_hr);
        _helperUnSelected(_hr);
        findLayer();
        setInfluence(0);
        setInfluence(_ControllerInfluence);
        _CheclInterSections();
        if (_activeHelpers.Count < 1)
        {
            InterActiveHelper = null;
            if (_activeEyeSelection == null)
                SetAxies(3, "");//Hide Axis
        }
        else
        {
            InterActiveHelper = _activeHelpers[_activeHelpers.Count - 1];
            MasterPosition = InterActiveHelper.OldPosition;
            SetAxies(0, "Points");//Movement Axis
        }
    }

    //set Visual Influence for selected Points-First Level of Neighbors
    void setFirstLayerInfluence(float a)
    {
        List<_bone> Connections = new List<_bone>();
        foreach (Helper hr in _activeHelpers)
        {
            foreach (_bone _bn in hr._Connections)
            {
                Connections.Add(_bn);
            }
            if (_symmetry && !hr.SymmetryFreeze)
            {
                foreach (_bone _bn in hr.hSymmetryHelper._Connections)
                {
                    Connections.Add(_bn);
                }
            }
        }
        Connections = Connections.Distinct().ToList();

        for (int i = 0; i < Connections.Count; i++)
        {
            Connections[i].Influence = a;
            Connections[i]._weighted = a == 0 ? false : true;
        }
    }
    //set Visual Influence for selected Points-other Levels of Neighbors
    public void setInfluence(float a)
    {
        if (MaskLvl == 0) return;
        setFirstLayerInfluence(a);
        setInfluence(_lvl[0], .5f * a);
        if (MaskLvl == 1) return;
        setInfluence(_lvl[1], .4f * a);
        if (MaskLvl == 2) return;
        setInfluence(_lvl[2], .31f * a);
        if (MaskLvl == 3) return;
        setInfluence(_lvl[3], .22f * a);
        if (MaskLvl == 4) return;
        setInfluence(_lvl[4], .13f * a);
        if (MaskLvl == 5) return;
        setInfluence(_lvl[5], .08f * a);
        if (MaskLvl == 6) return;
        setInfluence(_lvl[6], .06f * a);
        if (MaskLvl == 7) return;
        setInfluence(_lvl[7], .04f * a);
        if (MaskLvl == 8) return;
        setInfluence(_lvl[8], .03f * a);
        if (MaskLvl == 9) return;
        setInfluence(_lvl[9], .02f * a);
    }
    //check masking level
    public void _CheclInterSections()
    {
        if (MaskLvl < 10)
        {
            setInfluence(_lvl[MaskLvl], 0);
        }
    }
    //set Visual Influence for assigned Helpers
    void setInfluence(Helper[] _helpers, float influence)
    {
        if (_helpers.Length < 1)
            return;
        for (int i = 0; i < _helpers.Length; i++)
        {
            for (int j = 0; j < _helpers[i]._Connections.Count; j++)
            {
                if (influence == 0)
                {
                    _helpers[i]._Connections[j].Influence = 0;
                    _helpers[i]._Connections[j]._weighted = false;

                }
                else
                if (!_helpers[i]._Connections[j]._weighted)
                {
                    SetBoneInfluence(_helpers[i], _helpers[i]._Connections[j], influence);
                }
            }
            if (_symmetry && !_helpers[i].SymmetryFreeze)
            {
                for (int j = 0; j < _helpers[i].hSymmetryHelper._Connections.Count; j++)
                {
                    if (influence == 0)
                    {
                        _helpers[i].hSymmetryHelper._Connections[j].Influence = 0;
                        _helpers[i].hSymmetryHelper._Connections[j]._weighted = false;
                    }
                    else
                    if (!_helpers[i].hSymmetryHelper._Connections[j]._weighted)
                    {
                        SetBoneInfluence(_helpers[i].hSymmetryHelper, _helpers[i].hSymmetryHelper._Connections[j], influence);
                    }
                }
            }
        }
        //UpdatePositionConstraint();
    }
    //set Visual Influence for assigned Bones
    public void SetBoneInfluence(Helper _helper, _bone _bone, float influence)
    {
        switch (_activeHelpers[_activeHelpers.Count - 1]._type)
        {
            case FaceControllerType.OpenFace: if (_bone._PBoneCons) _bone.Influence = 0; else _bone.Influence += influence; break;
            case FaceControllerType.User: if (_bone._PBoneCons) _bone.Influence = 0; else _bone.Influence += influence; break;
            case FaceControllerType.Helper: if (_bone._PBoneCons || _bone._SBoneCons) _bone.Influence = 0; else _bone.Influence += influence; break;
        }
        _bone._weighted = true;
    }

    //Find Helpers by Level
    Helper[] _findHelper(Helper[] level)
    {
        List<Helper[]> _neighboarslvl = new List<Helper[]>();
        for (int i = 0; i < level.Length; i++)
        {
            switch (_activeHelpers[_activeHelpers.Count - 1]._type)
            {
                case FaceControllerType.Helper: _neighboarslvl.Add(level[i]._AHneighbors.ToArray()); break;
                case FaceControllerType.User: _neighboarslvl.Add(level[i]._Hneighbors.ToArray()); break;
                case FaceControllerType.OpenFace: _neighboarslvl.Add(level[i]._Hneighbors.ToArray()); break;
            }
        }

        Helper[] _lvl = _neighboarslvl.SelectMany(item => item).Distinct().ToArray().Except(_lvlfound.SelectMany(item => item).Distinct().ToArray()).ToArray();
        return _lvl;
    }

    //Find Neighbors Levels
    void findLayer()
    {
        _lvlfound.Clear();
        List<Helper> ne = new List<Helper>();
        foreach (Helper _hr in _activeHelpers)
        {
            switch (_hr._type)
            {
                case FaceControllerType.Helper:
                    ne.AddRange(_hr._AHneighbors);
                    break;
                case FaceControllerType.User:
                    ne.AddRange(_hr._Hneighbors);
                    break;
                case FaceControllerType.OpenFace:
                    ne.AddRange(_hr._Hneighbors);
                    break;
            }
        }
        _lvl[0] = ne.Distinct().ToArray();

        for (int i = 0; i < _lvl.Length - 1; i++)
        {
            _lvlfound.Add(_lvl[i]);
            _lvl[i + 1] = _findHelper(_lvl[i]);
        }

    }
    //set Weight for all selected Helpers
    public void SetWeightInfluenceForSelectedHelpers()
    {
        SetWeightInfluence(_ControllerInfluence);
        setInfluence(0);
        setInfluence(_ControllerInfluence);
        _CheclInterSections();
    }
    //set Weight for Selected Helpers
    private void SetWeightInfluence(float a)
    {
        List<Helper> Neibs = new List<Helper>();
        foreach (Helper hl in _activeHelpers)
        {
            foreach (Helper hr in hl._AHneighbors)
            {
                Neibs.Add(hr);
            }
        }
        Neibs = Neibs.Distinct().Except(_activeHelpers).ToList();
        foreach (Helper hr in Neibs)
        {
            if (hr._type != FaceControllerType.OpenFace)
                hr._PosConst.OverallWeight = a;
            if (_symmetry && !hr.SymmetryFreeze)
                if (hr.hSymmetryHelper._type != FaceControllerType.OpenFace)
                    hr.hSymmetryHelper._PosConst.OverallWeight = a;
        }
    }
    //set Weight for assigned Helper
    private void SetWeightInfluence(Helper hl, float a)
    {
        foreach (Helper hr in hl._AHneighbors)
        {
            if (hr._type != FaceControllerType.OpenFace)
                hr._PosConst.OverallWeight = a;
            if (_symmetry && !hr.SymmetryFreeze)
                if (hr.hSymmetryHelper._type != FaceControllerType.OpenFace)
                    hr.hSymmetryHelper._PosConst.OverallWeight = a;
        }
    }
    //Unset Weight Influence  for assigned Helper
    public void UnSetWeightInfluence(Helper _hr)
    {
        foreach (Helper hr in _hr._AHneighbors)
        {
            if (hr._type != FaceControllerType.OpenFace)
                hr._PosConst.OverallWeight = 1;
            if (_symmetry && !hr.SymmetryFreeze)
                if (hr.hSymmetryHelper._type != FaceControllerType.OpenFace)
                    if (!hr.hSymmetryHelper.activate)
                        hr.hSymmetryHelper._PosConst.OverallWeight = 1;
        }

    }

    int oldlvl = 0;//selected Masklvel
    //set Masking Levels
    public void SetMask()
    {
        if (oldlvl == MaskLvl)
            return;
        oldlvl = MaskLvl;
        setInfluence(0);
        setInfluence(_ControllerInfluence);
        for (int y = 0; y < _lvl.Length; y++)
        {
            foreach (Helper hl in _lvl[y])
            {
                UnSetWeightInfluence(hl);
            }
        }
        for (int y = 9; y >= MaskLvl; y--)
        {
            setInfluence(_lvl[y], 0);

            foreach (Helper hl in _lvl[y])
            {
                SetWeightInfluence(hl, 0);
            }
        }
        setFirstLayerInfluence(_ControllerInfluence);
    }

    //Create Morph Controllers
    public void CreateBlendShapes()
    {
        _blendShapeCount = _skinnedMesh.blendShapeCount;
        string oldActionName = "";
        string Name = "";
        string[] words;
        for (int i = 0; i < _blendShapeCount; i++)
        {
            Name = _skinnedMesh.GetBlendShapeName(i);
            words = Name.Split('_');
            _morpherController _mC;
            if (oldActionName != words[1])
            {
                oldActionName = words[1];
                GameObject MorphController = Instantiate(_MorphControllerPrefab, UI.Instance._MorphControllerContentParent, false);
                MorphController.name = words[0] + "_" + words[1];
                _mC = MorphController.GetComponent<_morpherController>();
                _mC._init(i);
                _MorphAll.Add(_mC);

                switch (words[0])
                {
                    case "Eyes": _MCEyes.Add(_mC); break;
                    case "NoseGlabella": _MCNose.Add(_mC); break;
                    case "LChZM": _MCLips.Add(_mC); break;
                    case "EarMandible": _MCEars.Add(_mC); break;
                    case "Skull": _MCSkull.Add(_mC); break;
                    case "Fat": _MCFat.Add(_mC); break;
                    case "Age": _MCAge.Add(_mC); break;
                    case "Sex": _MCSex.Add(_mC); break;
                    case "Race": _MCRace.Add(_mC); break;
                    case "Mist": _MCMist.Add(_mC); break;
                }
                if (words[1].Contains("Skull")) _MCSkull.Add(_mC);
                if (words[1].Contains("Fat")) _MCFat.Add(_mC);
                if (words[1].Contains("Aging")) _MCAge.Add(_mC);
                if (words[1].Contains("Sex")) _MCSex.Add(_mC);
                if (words[1].Contains("EyeLashes")) SetMorphEyeLashes(_mC, i);
            }
        }
        SelectMorphs(0);
    }


    //Select MorphGroups
    public void SelectMorphs(int i)
    {
        if (_selectedMorphGroup.Length > 0)
            setMorphActive(_selectedMorphGroup.ToArray(), false);
        switch (i)
        {
            case 0: _selectedMorphGroup = _MCEyes.ToArray(); break;
            case 1: _selectedMorphGroup = _MCNose.ToArray(); break;
            case 2: _selectedMorphGroup = _MCLips.ToArray(); break;
            case 3: _selectedMorphGroup = _MCEars.ToArray(); break;
            case 4: _selectedMorphGroup = _MCSkull.ToArray(); break;
            case 5: _selectedMorphGroup = _MCFat.ToArray(); break;
            case 6: _selectedMorphGroup = _MCAge.ToArray(); break;
            case 7: _selectedMorphGroup = _MCSex.ToArray(); break;
            case 8: _selectedMorphGroup = _MCRace.ToArray(); break;
            case 9: _selectedMorphGroup = _MCMist.ToArray(); break;
        }

        setMorphActive(_selectedMorphGroup.ToArray(), true);
    }
    //Turn On Morphs
    void setMorphActive(_morpherController[] _Array, bool _state)
    {
        for (int i = 0; i < _Array.Length; i++)
        {

            _Array[i].gameObject.SetActive(_state);
        }
    }
    //Turn on off Symmetry
    public void SetSymmetry()
    {
        switch (_i)
        {
            case 0:
                if (_activeHelperSelection == null)
                    break;
                _symmetry = !_symmetry;
                if (_symmetry)
                {
                    UI.Instance._buttons[32].image.color = UI.Instance._colors[1];
                    UI.Instance._images[0].color = UI.Instance._colors[1];
                    if (_activeHelperSelection.SymmetryFreeze)
                        break;
                    _activeHelperSelection._HSSymmetry._selected = true;
                    _activeHelperSelection._HSSymmetry.transform.localScale = _activeHelperSelection._HSSymmetry.transform.localScale * _activeHelperSelection._HSSymmetry._scale;
                }
                else
                {
                    UI.Instance._buttons[32].image.color = UI.Instance._colors[0];
                    UI.Instance._images[0].color = UI.Instance._colors[0];
                    if (_activeHelperSelection.SymmetryFreeze)
                        break;
                    _activeHelperSelection._HSSymmetry._selected = false;
                    _activeHelperSelection._HSSymmetry.transform.localScale = _activeHelperSelection._HSSymmetry.transform.localScale / _activeHelperSelection._HSSymmetry._scale;
                }

                break;
            case 1:
            case 2:
                if (_activeEyeSelection != null)
                {
                    _symmetry = !_symmetry;
                    UI.Instance._buttons[34].image.color = _symmetry ? UI.Instance._colors[1] : UI.Instance._colors[0];
                    UI.Instance._images[0].color = _symmetry ? UI.Instance._colors[1] : UI.Instance._colors[0];
                    break;
                }
                if (_activeHelpers.Count < 1)
                    break;
                setInfluence(0);
                _symmetry = !_symmetry;
                setInfluence(_ControllerInfluence);
                _CheclInterSections();
                if (_symmetry)
                {
                    UI.Instance._buttons[28].image.color = UI.Instance._colors[1];
                    UI.Instance._images[0].color = UI.Instance._colors[1];
                    foreach (Helper _hr in _activeHelpers)
                        if (!_hr.SymmetryFreeze)
                        {
                            _hr.hSymmetryHelper.activate = true;
                            _hr.hSymmetryHelper.transform.localScale = _hr.hSymmetryHelper.transform.localScale * _hr._scale;
                            _hr.hSymmetryHelper._mr.material.color = _hr.ColorCode[3];
                            _hr.hSymmetryHelper._selected = true;
                            _hr.hSymmetryHelper.OldPosition = _hr.hSymmetryHelper.Master.transform.position;
                            if (_hr._type != FaceControllerType.OpenFace)
                                _hr.hSymmetryHelper._PosConst.OverallWeight = 0;
                        }
                }
                else
                {
                    UI.Instance._buttons[28].image.color = UI.Instance._colors[0];
                    UI.Instance._images[0].color = UI.Instance._colors[0];
                    foreach (Helper _hr in _activeHelpers)
                        if (!_hr.SymmetryFreeze)
                        {
                            UnSetWeightInfluence(_hr.hSymmetryHelper);
                            _hr.hSymmetryHelper._Unselect();
                            _hr.hSymmetryHelper.activate = false;
                            if (_hr._type != FaceControllerType.OpenFace)
                                _hr.hSymmetryHelper._PosConst.OverallWeight = 0;
                        }
                }
                break;
        }
    }
    //Make Selected Points SymmetryCall
    public void MakeSymmetry()
    {
        switch (_i)
        {
            case 0:
                if (_activeHelperSelection != null)
                {
                    if (!_activeHelperSelection.SymmetryFreeze)
                    {
                        _activeHelperSelection._HSSymmetry.Master.position = new Vector3(
                         -_activeHelperSelection.Master.position.x, _activeHelperSelection.Master.position.y, _activeHelperSelection.Master.position.z);
                        _activeHelperSelection._HSSymmetry.Master.eulerAngles = new Vector3(
                            _activeHelperSelection.Master.eulerAngles.x, -_activeHelperSelection.Master.eulerAngles.y, -_activeHelperSelection.Master.eulerAngles.z);
                    }
                    break;
                }
                break;
            case 1:
            case 2:
                if (_activeEyeSelection != null)
                {
                    _activeEyeSelection.SymEye.transform.position = new Vector3(
                         -_activeEyeSelection.transform.position.x, _activeEyeSelection.transform.position.y, _activeEyeSelection.transform.position.z);
                    _activeEyeSelection.SymEye.transform.eulerAngles = new Vector3(
                        _activeEyeSelection.transform.eulerAngles.x, -_activeEyeSelection.transform.eulerAngles.y, -_activeEyeSelection.transform.eulerAngles.z);
                    _activeEyeSelection.SymEye.transform.localScale = _activeEyeSelection.transform.localScale;
                    break;
                }
                if (InterActiveHelper == null)
                    return;
                foreach (Helper _hr in _activeHelpers)
                    if (!_hr.hSymmetryHelper.SymmetryFreeze)
                    {
                        if (_hr.hSymmetryHelper._type != FaceControllerType.OpenFace)
                            _hr.hSymmetryHelper._PosConst.OverallWeight = 0;
                        _hr.SymmetryHelper.transform.position = new Vector3(-_hr.transform.position.x, _hr.transform.position.y, _hr.transform.position.z);
                    }
                if (!_symmetry)
                    foreach (Helper _hr in _activeHelpers)
                        if (_hr.hSymmetryHelper._type != FaceControllerType.OpenFace)
                            StartCoroutine(weightBackHelpers(_hr.hSymmetryHelper, .5f));
                break;
        }
    }

    //Set Morphs ForEyeLashes
    void SetMorphEyeLashes(_morpherController _mC, int i)
    {
        int n = i;
        _mC.SliderC.onValueChanged.AddListener(delegate
        {
            _eyeLashesSkinnedMeshRenderer.SetBlendShapeWeight(0, _mC.SliderC.value * 100);
        });
    }

    //Show Axies 0-Eyes 1-Selection 2-Points 3-Hide
    public void SetAxies(int i, string _type)
    {
        if (i == 3)
        {
            _AxisController.Hide();
        }
        else
        {
            _AxisController.Eyes = false;
            _AxisController.Selection = false;
            _AxisController.Points = false;
            switch (_type)
            {
                case "Eyes":
                    _AxisController.Eyes = true;
                    _AxisController.transform.parent = _activeEyeSelection.transform;
                    break;
                case "Selection":
                    _AxisController.Selection = true;
                    _AxisController.transform.parent = _activeHelperSelection.transform;
                    break;
                case "Points":
                    _AxisController.Points = true;
                    _AxisController.transform.parent = InterActiveHelper.transform;
                    rotation = false;
                    break;
            }
            _AxisController.transform.localPosition = Vector3.zero;
            _AxisController.transform.eulerAngles = Vector3.zero;
            _AxisController.transform.localScale = Vector3.one;
            if (rotation)
                SetTransform(1);
            else
                SetTransform(0);
        }
    }
    /// <summary>
    /// setAxis 0-Move 1-Rotation 2-Scale 3-Hide
    /// </summary>
    /// <param Id="t"></param>
    public void SetTransform(int t)
    {
        movement = false;
        rotation = false;
        scale = false;
        UI.Instance._buttons[36].image.color = UI.Instance._colors[0];
        UI.Instance._buttons[37].image.color = UI.Instance._colors[0];
        UI.Instance._buttons[38].image.color = UI.Instance._colors[0];
        UI.Instance._buttons[39].image.color = UI.Instance._colors[0];
        switch (t)
        {
            case 0:
                movement = true;
                UI.Instance._buttons[36].image.color = UI.Instance._colors[1];
                UI.Instance._buttons[38].image.color = UI.Instance._colors[1];
                _AxisController.Show(0);
                break;
            case 1:
                rotation = true;
                UI.Instance._buttons[37].image.color = UI.Instance._colors[1];
                UI.Instance._buttons[39].image.color = UI.Instance._colors[1];
                _AxisController.Show(1);
                break;
            case 2:
                break;
        }
    }

    public void CheckSymmetry()
    {
        switch (_i)
        {
            case 0:
                UI.Instance._buttons[32].image.color = _symmetry ? UI.Instance._colors[1] : UI.Instance._colors[0];
                break;
            case 1:
            case 2:
                UI.Instance._buttons[28].image.color = _symmetry ? UI.Instance._colors[1] : UI.Instance._colors[0];
                UI.Instance._buttons[34].image.color = _symmetry ? UI.Instance._colors[1] : UI.Instance._colors[0];
                break;
        }
    }
    public void ResetSelectionPoints()
    {
        foreach (_HelperSelections _hs in _HelpersSelection)
            _hs.TempReset();
    }
    public void ResetEyes()
    {
        foreach (_eyeHelperSelection _eye in _eyeHelperSelections)
            _eye.TempReset();
    }
    public void ResetAllThePoints()
    {
        foreach (Helper _h in _Helpers)
        {
            _h.TempResetPosition();
            if (_h._type != FaceControllerType.OpenFace)
            {
                _h._PosConst.OverallWeight = 0;
                StartCoroutine(weightBackHelpers(_h, 0));
            }
        }
    }
    public void ResetOpenFacePoints()
    {
        foreach (Helper _h in _Helpers)
        {
            if (_h._type == FaceControllerType.OpenFace)
            {
                _h.TempResetPosition();
            }
        }
    }

    public void ResetUserPoints()
    {
        foreach (Helper _h in _Helpers)
        {
            if (_h._type == FaceControllerType.User)
            {
                _h._PosConst.OverallWeight = 0;
                _h.TempResetPosition();
                StartCoroutine(weightBackHelpers(_h, 0));
            }
        }
    }

    public void ResetHelperPoints()
    {
        foreach (Helper _h in _Helpers)
        {
            if (_h._type == FaceControllerType.Helper)
            {
                _h._PosConst.OverallWeight = 0;
                _h.TempResetPosition();
                StartCoroutine(weightBackHelpers(_h, 0));
            }
        }
    }

    public IEnumerator weightBackHelpers(Helper _hr, float delay)
    {
        if (delay == 0)
            yield return new WaitForEndOfFrame();
        else
            yield return new WaitForSeconds(delay);
        if (_hr._type != FaceControllerType.OpenFace)
            _hr._PosConst.OverallWeight = 1;
    }


    public void Relax()
    {
        return;
        Mesh m = new Mesh();
        _MeshHead.localEulerAngles = Vector3.zero;
        _skinnedMeshRenderer.BakeMesh(m);
        _skinnedMeshRenderer.sharedMesh = mattatz.MeshSmoothingSystem.MeshSmoothing.LaplacianFilter(m, 1);
        _MeshHead.localEulerAngles = new Vector3(-90, 0, 0);
    }

    public void Fix()
    {

    }
    public void SetFaceAsDefault()
    {
        foreach (_HelperSelections _hs in _HelpersSelection)
            _hs.SetAsDefault();
        foreach (_eyeHelperSelection _eye in _eyeHelperSelections)
            _eye.SetAsDefault();
        foreach (Helper _hr in _Helpers)
            _hr.setAsDefault();
        UI.Instance.SetSliderAsDefault();
    }
    public void ResetCatagorieMorphs()
    {
        foreach (_morpherController _mc in _selectedMorphGroup)
            _mc.TempReset();
    }
    public void ResetAllMorphs()
    {
        foreach (_morpherController _mc in _MorphAll)
            _mc.TempReset();
    }
    public void MorphSetAsDefault()
    {
        foreach (_morpherController _mc in _MorphAll)
            _mc.SetAsDefault();
    }
    public void ResetActiveEye()
    {
        _activeEyeSelection.TempReset();
        if (_symmetry)
            _activeEyeSelection.SymEye.TempReset();
        resetMasterTransform(_activeEyeSelection.OldPosition, _activeEyeSelection.OldRotation, _activeEyeSelection.OldScale.x);
    }
    public void ResetActiveSelection()
    {
        _activeHelperSelection.TempReset();
        if (_symmetry)
            _activeHelperSelection._HSSymmetry.TempReset();
        resetMasterTransform(_activeHelperSelection.OldPosition, _activeHelperSelection.OldRotation, _activeHelperSelection.OldScale.x);
    }
    public void ResetActiveHelpers()
    {
        foreach (Helper _hr in _activeHelpers)
            _hr.TempResetPosition();
        if (_symmetry)
            foreach (Helper _hr in _activeHelpers)
                if (!_hr.SymmetryFreeze)
                    _hr.hSymmetryHelper.TempResetPosition();
        resetMasterTransform(InterActiveHelper.OldPosition,Quaternion.identity, 1);
    }

    void resetMasterTransform(Vector3 pos, Quaternion rot, float Scale)
    {
        MasterPosition = pos;
        MasterRotation = rot;
        MasterScale = Scale;
    }
    private bool eyeProjection = false;
    public void EnableEyeProjection()
    {
        eyeProjection = !eyeProjection;
        foreach (_eyeHelperSelection e in _eyeHelperSelections)
            if (eyeProjection)
                e.gameObject.layer = 9;
            else
                e.gameObject.layer = 12;
    }



    public void ShowHideScan()
    {
        if (Scan == null)
            return;
        MeshRenderer mr = Scan.transform.GetComponentInChildren<MeshRenderer>();
        if (mr.enabled)
            mr.enabled = false;
        else
            mr.enabled = true;
    }

}











public enum BlendMode
{
    Opaque,
    Cutout,
    Fade,
    Transparent
}
public static class StandardShaderUtils
{
    public static void ChangeRenderMode(Material standardShaderMaterial, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = -1;
                break;
            case BlendMode.Cutout:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                standardShaderMaterial.SetInt("_ZWrite", 1);
                standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 2450;
                break;
            case BlendMode.Fade:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
            case BlendMode.Transparent:
                standardShaderMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                standardShaderMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                standardShaderMaterial.SetInt("_ZWrite", 0);
                standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
                standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
                standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                standardShaderMaterial.renderQueue = 3000;
                break;
        }

    }

}

enum RigType{
    head,
    Body,
    Generic
    }