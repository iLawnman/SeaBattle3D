﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class BattleGameManager : MonoBehaviour
{
    //game data
    public enum GameMode {pause, edit, play};
    public GameMode currentMode;
    public Text modeInfo;

    //game ui elements
    public GameObject cross;
    public GameObject hitShip;
    public bool playerCanShot = true;

    //AI ships data
    public PlayerField otherPlayerFeildData;
    public GameObject otherPlayerField;
    public List<Collider> otherShipColliders;
    public List<Vector3> aiShoots = new List<Vector3>();

    //Player ships data
    public GameObject playerField;
    public PlayerField playerFieldData;
    public List<Collider> playerShipColliders;

    //data for edit mode
    public GameObject editedShip;
    public bool editCanPlace;

    //game data
    public GameUIManager uiManager;
    public GameAudioManager audioManager;
    public AudioSource aSource;

    //input data
    public Vector2 _mousePosition;
    private SeaBattleInputAction _input;

    public Text touchTxt;

    // Start is called before the first frame update
    void Start()
    {
        uiManager = FindObjectOfType<GameUIManager>();
        audioManager = FindObjectOfType<GameAudioManager>();
        aSource = GetComponent<AudioSource>();
        playerShipColliders = GetShipColliders(playerFieldData);
        otherShipColliders = GetShipColliders(otherPlayerFeildData);
    }

    void Awake()
    {
        _input = new SeaBattleInputAction();
        _input.Player.Move.performed += Move_performed;
    }

    private void OnEnable()
    {
        _input.Enable();
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
        EnhancedTouchSupport.Disable();
    }

    public void DragAsset(Touch touch)
    {
        if (touch.phase == TouchPhase.Moved)
        {
            touchTxt.text = "MOVE";
        }
    }

    //get mouse screen position
    private void Move_performed(InputAction.CallbackContext obj)
    {
        _mousePosition = obj.ReadValue<Vector2>();
        touchTxt.text = _mousePosition.ToString();
    }

    // get colliders of all ships on game field
    List<Collider> GetShipColliders (PlayerField field)
    {
        List<Collider> allColl = new List<Collider>();

        foreach (Transform shi in field.transform)
        {

            allColl.AddRange(shi.GetComponentsInChildren<Collider>());
        }
        return allColl;
    }

    // Place ships on random place on AI field
    void AIShipsRandomPlace ()
    {
        var otherShips = otherPlayerFeildData.Ships;

        foreach (PlayerField.ShipData ship in otherShips)
        {
            if (Random.Range(0, 2) == 1)
                ship.go.transform.RotateAround(ship.go.transform.position, transform.up, 90);
            
            PlaceAIShip(ship.go);
        }
    }

    // try place ai ships on right place
    void PlaceAIShip(GameObject ship)
    {
        do
        {
            ship.transform.localPosition = GetRandomCell();
        }
        while (!CheckShipCell(ship, otherPlayerFeildData));
    }

    // random cell on game field
    Vector3 GetRandomCell ()
    {
        return
               new Vector3(Random.Range(0, 10), 0.5f, Random.Range(-9, 1));
    }

    // check place for in field and don't touch other ships
    bool CheckShipCell (GameObject ship, PlayerField playerField)
    {
        Collider[] cols = ship.GetComponentsInChildren<Collider>();

        foreach (Collider chi in cols)
        {
            Vector3 chiRelative = playerField.transform.InverseTransformPoint(chi.transform.position);
            if (chiRelative.x < 0 || chiRelative.x > 9 || chiRelative.z > 0 || chiRelative.z < -9)
            {
                return false;
            }

            foreach (Collider othership in otherShipColliders)
            {
                if (chi != othership && chi.transform.parent.gameObject != othership.transform.parent.gameObject &&
                    (
                    chi.transform.position == othership.transform.position
                    || chi.transform.position + Vector3.up == othership.transform.position
                    || chi.transform.position + Vector3.down == othership.transform.position
                    || chi.transform.position + Vector3.forward == othership.transform.position
                    || chi.transform.position + Vector3.back == othership.transform.position
                    || chi.transform.position + Vector3.right == othership.transform.position
                    || chi.transform.position + Vector3.left == othership.transform.position
                    || chi.transform.position + Vector3.left + Vector3.up == othership.transform.position
                    || chi.transform.position + Vector3.left + Vector3.down == othership.transform.position
                    || chi.transform.position + Vector3.right + Vector3.up == othership.transform.position
                    || chi.transform.position + Vector3.right + Vector3.down == othership.transform.position
                    || chi.transform.position + Vector3.left + Vector3.forward == othership.transform.position
                    || chi.transform.position + Vector3.left + Vector3.back == othership.transform.position
                    || chi.transform.position + Vector3.right + Vector3.forward == othership.transform.position
                    || chi.transform.position + Vector3.right + Vector3.back == othership.transform.position
                    )
                   )
                {
                    return false;
                }
            }
        }
            return true;
    }

    //check for player ship die
    bool CheckPlayerShipDie (Transform ship)
    {
        foreach (Transform chi in ship)
        {
            if (chi.localPosition.y == 0)

                return false;
        }
        return true;
    }

    // check ai shot for injured or die player ship
    bool CheckAIShipShoot(Vector3 shoot)
    {
        // check cubes transform == shoot
        foreach (Collider cube in playerShipColliders)
        {
            if (cube.CompareTag("Ship"))
            {
                if (cube.transform.position.x == shoot.x && cube.transform.position.z == shoot.z)
                {
                    Debug.Log("AI shoot in " + cube.transform.parent.name);
                    aSource.PlayOneShot(audioManager.ship);

                    var ship = playerFieldData.Ships.Find(x => x.go == cube.transform.parent.gameObject);
                    
                    cube.transform.position += new Vector3(0, -0.2f, 0);
                    ship.status = PlayerField.shipStatus.Injured;

                    if (CheckPlayerShipDie(ship.go.transform))
                    {
                        ship.status = PlayerField.shipStatus.Die;
                        AddDeadZone(ship.go);
                        playerFieldData.CheckGameStatus();
                    }
                    else 
                        ship.status = PlayerField.shipStatus.Injured;

                    return true;
                }
            }
        }
        return false;
    }

    // instance shot GO
    void AIshot()
    {
        Vector3 shootAIposition = GetAIshot();

        GameObject fire = Instantiate(cross);
        fire.transform.SetParent(playerFieldData.transform);
        fire.transform.localPosition = shootAIposition;

        if (CheckAIShipShoot(fire.transform.position))
        {
            GameObject shipfire = Instantiate(hitShip);

            shipfire.transform.SetParent(playerFieldData.transform);
            shipfire.transform.localPosition = shootAIposition + new Vector3(0, 0.5f, 0);
            fire.transform.position += new Vector3(0, 0.5f, 0);

            playerCanShot = false;
        }
        else
        {
            playerCanShot = true;
        }
    }

    // getc ai shot position for random/injured player ship
    Vector3 GetAIshot()
    {
        Vector3 shoot = new Vector3();
        List<Vector3> goodShot = new List<Vector3>();

        var injShip = playerFieldData.Ships.Find(x => x.status == PlayerField.shipStatus.Injured);

        if (injShip == null && aiShoots.Count > 0)
        {
            do
            {
                shoot = new Vector3(Random.Range(0, 10), 0, Random.Range(-9, 1));

            } while (aiShoots.Contains(shoot));
        }

        else {
            shoot = new Vector3(Random.Range(0, 10), 0, Random.Range(-9, 1));
        }
        if (injShip != null && aiShoots.Count > 0) {

            foreach (Transform cube in injShip.go.transform)
            {
                Vector3 cubeRelative = playerFieldData.transform.InverseTransformPoint(cube.transform.position);
                cubeRelative = new Vector3(cubeRelative.x, 0, cubeRelative.z);
                if (cube.transform.localPosition.y < 0)
                    goodShot.Add(cubeRelative);
            }
                if (goodShot.Count == 1)
                {
                do
                {
                    var shift = Random.Range(0, 4);
                    switch (shift)
                    {
                        case 0:
                            shoot = goodShot[0] + Vector3.forward;
                            break;
                        case 1:
                            shoot = goodShot[0] + Vector3.back;
                            break;
                        case 2:
                            shoot = goodShot[0] + Vector3.left;
                            break;
                        case 3:
                            shoot = goodShot[0] + Vector3.right;
                            break;
                    }

                    if (aiShoots.Contains(shoot))
                        Debug.Log("1 inj sector shot contain new - new recall " + shoot);
                }
                while (aiShoots.Contains(shoot));
                }

            if (goodShot.Count > 1)
            {
                foreach (Transform cube in injShip.go.transform)
                {
                    Vector3 cubeRelative = playerFieldData.transform.InverseTransformPoint(cube.transform.position);
                    cubeRelative = new Vector3(cubeRelative.x, 0, cubeRelative.z);
                    if (cube.transform.localPosition.y == 0 && !aiShoots.Contains(new Vector3(cubeRelative.x, 0, cubeRelative.z)))
                        shoot = new Vector3(cubeRelative.x, 0, cubeRelative.z);
                    Debug.Log("2 and more inj " + shoot);
                }
            }
        }
        aiShoots.Add(new Vector3( shoot.x, 0, shoot.z));
        return new Vector3(shoot.x, 0, shoot.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (Touch.activeFingers.Count == 1)
        {
            _mousePosition = Touch.activeFingers[0].screenPosition;
        }
        Ray ray = Camera.main.ScreenPointToRay(_mousePosition);

        RaycastHit hit;

        if (currentMode == GameMode.play)
        {
            //***
            if (playerCanShot && _input.Player.Fire.triggered)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    aSource.PlayOneShot(audioManager.shot);
                    CheckFireCell(hit);
                }
            }
            if (!playerCanShot)
                AIshot();
        }
            if (currentMode == GameMode.edit)
            {
                if (Physics.Raycast(ray, out hit))
            {
                var rootCell = Vector3Int.RoundToInt(hit.point);

                touchTxt.text = rootCell.ToString();

                if (hit.collider.CompareTag("Player"))
                {
                    ShowShip(rootCell);
                }
                if (editCanPlace && rootCell != null && _input.Player.Fire.triggered)
                {
                    PlaceShip();
                }
                if (_input.Player.Rotate.triggered)
                {
                    RotateShip(rootCell);
                }
            }
        }
        
        //Two fingers means the player is trying to zoom in/out
        else if (Touch.activeFingers.Count == 2)
        {
            DragAsset(Touch.activeTouches[1]);

        }
        //No fingers while isBuilding is true means the player was dragging a model and stopped
        //else if (Touch.activeFingers.Count == 0)
        //{
        //   // touchTxt.text = "";
        //}
    }

    // end game
    public void EndGame (GameObject player)
    {
        Debug.Log(player.name + " LOSE!");

        string playerName = player.name;

        if (playerName.Contains("Player1"))
                PlayerLose();
             
           else
                PlayerWin();
    }

    public void QuitGame ()
    {
        Application.Quit();
    }

    //new game
    public void NewGame()
    {

        var otherShips = otherPlayerFeildData.Ships;
        foreach (PlayerField.ShipData oship in otherShips)
        {
            oship.status = PlayerField.shipStatus.Hide;
            ResetShipCubes(oship.go);
            foreach (Transform mesh in oship.go.transform)
            {
                mesh.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        AIShipsRandomPlace();

        var playerShips = playerFieldData.Ships;
        foreach (PlayerField.ShipData pship in playerShips)
        {
            pship.status = PlayerField.shipStatus.Hide;
            pship.go.transform.localPosition = new Vector3(2, 0.5f, -11);
            ResetShipCubes(pship.go);
        }
        var startShip = playerFieldData.Ships.Find(x => x.status == PlayerField.shipStatus.Hide);
        editedShip = startShip.go;

        var childs = GetComponentsInChildren<Transform>();

        foreach (Transform chi in childs)
        {
            if (chi.name.Contains("cross"))
                Destroy(chi.gameObject);
        }
        aiShoots.Clear();
        currentMode = GameMode.edit;

        uiManager.startUI.SetActive(false);
        uiManager.winUI.SetActive(false);
        uiManager.loseUI.SetActive(false);
    }

    // reset ships cubes
    void ResetShipCubes(GameObject ship)
    {
        foreach (Transform cube in ship.transform)
        {
            var cubeTransform = cube.transform;
            cube.position = new Vector3(cubeTransform.position.x, 0, cubeTransform.position.z);
        }
    }

    // player win
    void PlayerWin ()
    {
        currentMode = GameMode.pause;
        uiManager.winUI.SetActive(true);
    }

    // ai win
    void PlayerLose ()
    {
        currentMode = GameMode.pause;
        uiManager.loseUI.SetActive(true);
    }

    //place player ship and get next for select place
    void PlaceShip()
    {
        // select next ship
        var ship = playerFieldData.Ships.Find(x => x.go == editedShip);
        ship.status = PlayerField.shipStatus.Placed;
        var newShip = playerFieldData.Ships.Find(x => x.status == PlayerField.shipStatus.Hide);
        if (newShip != null)
        {
            var chiMeshs = newShip.go.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer chi in chiMeshs)
                chi.enabled = true;

            editedShip = newShip.go;
            editedShip.transform.localPosition = new Vector3(0, 0.5f, 0);

        }
        else
        {
            Debug.Log("Ready for play");
            currentMode = GameMode.play;
            modeInfo.text = "PLAY";
        }
    }

    // rotate player ship
    void RotateShip(Vector3Int rootCell)
    {
        editedShip.transform.RotateAround(rootCell, transform.up, 90);
    }

    // show ship while edit
    void ShowShip (Vector3Int cell)
    {
        CheckPlaceForShip(cell);
        CheckOtherShips();
    }

    // check edited ship for intersect with placed player ships
    bool CheckOtherShips ()
    {
        //check around for specfic ship
        Collider[] shipcols = editedShip.GetComponentsInChildren<Collider>();

        foreach (Collider chi in shipcols)
        {
                foreach (Collider othership in playerShipColliders)
                {
                    if (chi != othership && chi.transform.parent.gameObject != othership.transform.parent.gameObject &&
                        (
                        chi.transform.position == othership.transform.position
                        || chi.transform.position + Vector3.up == othership.transform.position
                        || chi.transform.position + Vector3.down == othership.transform.position
                        || chi.transform.position + Vector3.forward == othership.transform.position
                        || chi.transform.position + Vector3.back == othership.transform.position
                        || chi.transform.position + Vector3.right == othership.transform.position
                        || chi.transform.position + Vector3.left == othership.transform.position
                        || chi.transform.position + Vector3.left + Vector3.up == othership.transform.position
                        || chi.transform.position + Vector3.left + Vector3.down == othership.transform.position
                        || chi.transform.position + Vector3.right + Vector3.up == othership.transform.position
                        || chi.transform.position + Vector3.right + Vector3.down == othership.transform.position
                        || chi.transform.position + Vector3.left + Vector3.forward == othership.transform.position
                        || chi.transform.position + Vector3.left + Vector3.back == othership.transform.position
                        || chi.transform.position + Vector3.right + Vector3.forward == othership.transform.position
                        || chi.transform.position + Vector3.right + Vector3.back == othership.transform.position
                                                    )
                        )
                    {
                        //Debug.Log("Intersect with " + othership.transform.parent.name);
                        editCanPlace = false;
                        return false;
                    }
                }
        }
        return true;
    }

    // check edited ship in game field
    bool CheckPlaceForShip(Vector3Int cell)
    {
        editedShip.transform.position = cell;

        Collider[] cols = editedShip.GetComponentsInChildren<Collider>();
        var curColor = editedShip.GetComponentInChildren<MeshRenderer>().material.color;

        foreach (Collider chi in cols)
        {
            Vector3 chiRelative = playerField.transform.InverseTransformPoint(chi.transform.position);
            if (chiRelative.x < -0.5 || chiRelative.x > 0.5 || chiRelative.z > 0.5 || chiRelative.z < -0.5)
            {
                chi.GetComponent<MeshRenderer>().material.color = Color.red;
                editCanPlace = false;
                //Debug.Log("out of field");
                //return false;
            }
            else
            {
                chi.GetComponent<MeshRenderer>().material.color = curColor;
                editCanPlace = true;
            }
        }
        return true;
    }

    // check player fire cell for injured or die ai ship
    void CheckFireCell(RaycastHit hit) {

            if (hit.collider.CompareTag("Ship"))
            {
                aSource.PlayOneShot(audioManager.ship);

                GameObject shipInjured = Instantiate(hitShip);
                shipInjured.transform.localPosition = Vector3Int.RoundToInt(hit.point);
                shipInjured.transform.SetParent(otherPlayerField.transform);

                hit.collider.GetComponentInChildren<MeshRenderer>().enabled = true;

                var ship = otherPlayerFeildData.Ships.Find(x => x.go == hit.collider.transform.parent.gameObject);
                ship.status = PlayerField.shipStatus.Injured;

            // check ship status
            if (CheckShipDie(hit.collider.transform.parent))
                {
                    aSource.PlayOneShot(audioManager.ship);

                    hit.collider.transform.parent.position -= new Vector3(0, 0.2f, 0);
                    // set die in playerfield
                    ship.status = PlayerField.shipStatus.Die;
                    Debug.Log("ship " + ship.go.name + " " + ship.status);
                    otherPlayerFeildData.CheckGameStatus();
                }
            }
            if (hit.collider.CompareTag("Field"))
            {
                // check fire in place
                var checkCell = Vector3Int.RoundToInt(hit.point);

                if (!CheckFire(checkCell))
                {
                    aSource.PlayOneShot(audioManager.empty);

                    GameObject fire = Instantiate(cross);
                    fire.transform.localPosition = checkCell + new Vector3(0, -0.4f,0);
                    fire.transform.SetParent(otherPlayerField.transform);
                }
                playerCanShot = false;
            }
        }

    // add cells around died player ship for don't shot in
    void AddDeadZone (GameObject deadShip)
    {
        foreach (Transform cube in deadShip.transform)
        {
            //relative pos
            var relativeCube = playerFieldData.transform.InverseTransformPoint(cube.transform.position);
            
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.forward);
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.back);
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.left);
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.right);
                //add diagonals
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.forward + Vector3.left);
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.back + Vector3.left);
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.forward + Vector3.right);
                aiShoots.Add(new Vector3(relativeCube.x, 0, relativeCube.z) + Vector3.back + Vector3.right);
        }
        aiShoots = aiShoots.Distinct().ToList();
    }

    //check player fire for ai ship
    bool CheckFire(Vector3Int checkin)
        {
            foreach (Transform chi in otherPlayerField.transform)
            {
                if (chi.position == checkin)
                    return true;
            }
            return false;
        }

    // check ai ship for die
    bool CheckShipDie(Transform ship)
        {
            foreach (Transform chi in ship)
            {
                if (chi.GetComponentInChildren<MeshRenderer>().enabled == false)

                    return false;
            }
            return true;
        }

    }