using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class character : MonoBehaviour
{
    public CoreScript CS;
    public bool MainPlayer;
    public float MoveSpeed;
    public float KnockbackPower;
    public float JumpPower;
    public float DownPower;
    public float AttackSpeed;
    public bool OnPlatform;
    public bool AttackingOut;
    public bool AttackingIn;
    public bool FacingRight;
    public bool AttackingRight;
    public bool AttackingLeft;
    public bool AttackingUp;
    public bool AttackingDown;
    public int HurtAmount;
    public int p1deathcount;
    public int p2deathcount;
    public GameObject p1health;
    public GameObject p2health;
    public GameObject p1deaths;
    public GameObject p2deaths;

    public GameObject Arm;

    // Start is called before the first frame update
    void Start()
    {
        CS = GameObject.Find("GameController").GetComponent<CoreScript>();
        Arm = gameObject.transform.GetChild(0).gameObject;
        AttackingOut = false;
        AttackingIn = false;
        AttackingRight = false;
        HurtAmount = 0;
        p1deathcount = 0;
        p2deathcount = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "platform")
        {
            OnPlatform = true;
        }
        if (collision.gameObject.tag == "border")//this will be used to kill players if they fall off map, for test teleports to mid
        {
            gameObject.transform.position = new Vector2(0, -2);
            HurtAmount = 0;

            if (MainPlayer)
            {
                p1deathcount += 1;
                p1deaths.GetComponent<Text>().text = p1deathcount.ToString();
                p1health.GetComponent<Text>().text = HurtAmount.ToString();
            }
            else
            {
                p2deathcount += 1;
                p2deaths.GetComponent<Text>().text = p2deathcount.ToString();
                p2health.GetComponent<Text>().text = HurtAmount.ToString();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "weapon" && collision.gameObject.GetComponentInParent<character>().AttackingRight)
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.right * KnockbackPower * HurtAmount);
            HurtAmount += 1;
            if (MainPlayer)
            {
                p1health.GetComponent<Text>().text = HurtAmount.ToString();
            }
            else
            {
                p2health.GetComponent<Text>().text = HurtAmount.ToString();
            }
        }
        else if (collision.gameObject.tag == "weapon" && collision.gameObject.GetComponentInParent<character>().AttackingLeft)
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce((transform.right * KnockbackPower) * -1 * HurtAmount);
            HurtAmount += 1;
            if (MainPlayer)
            {
                p1health.GetComponent<Text>().text = HurtAmount.ToString();
            }
            else
            {
                p2health.GetComponent<Text>().text = HurtAmount.ToString();
            }
        }
        else if (collision.gameObject.tag == "weapon" && collision.gameObject.GetComponentInParent<character>().AttackingUp)
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce((transform.up * KnockbackPower) * HurtAmount);
            HurtAmount += 1;
            if (MainPlayer)
            {
                p1health.GetComponent<Text>().text = HurtAmount.ToString();
            }
            else
            {
                p2health.GetComponent<Text>().text = HurtAmount.ToString();
            }
        }
        else if (collision.gameObject.tag == "weapon" && collision.gameObject.GetComponentInParent<character>().AttackingDown)
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce((transform.up * KnockbackPower) * -1 * HurtAmount);
            HurtAmount += 1;
            if (MainPlayer)
            {
                p1health.GetComponent<Text>().text = HurtAmount.ToString();
            }
            else
            {
                p2health.GetComponent<Text>().text = HurtAmount.ToString();
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (MainPlayer)
        {
            Movement(); //move to fixed update
            AttackingFunction();
        }
        else
        {
            //MovementPlayer2();
            //AttackingFunctionPlayer2();
        }
    }

    void Movement()
    {

        if (Input.GetKey(KeyCode.A))
        {
            CS.SendInputData(KeyCode.A);
            //send to trying move left. 
            gameObject.GetComponent<Rigidbody2D>().AddForce((transform.right * MoveSpeed) * -1);
            FacingRight = false;
        }
        if (Input.GetKey(KeyCode.D))
        {
            CS.SendInputData(KeyCode.D);
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.right * MoveSpeed);
            FacingRight = true;
        }
        if (Input.GetKeyDown(KeyCode.W) && OnPlatform)
        {
            CS.SendInputData(KeyCode.W);
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * JumpPower);
            OnPlatform = false;
        }
        if (Input.GetKey(KeyCode.S) && !OnPlatform)
        {
            CS.SendInputData(KeyCode.S);
            gameObject.GetComponent<Rigidbody2D>().AddForce((transform.up * DownPower) * -1);
        }
    }

    public void MovementPlayer2(string key)
    {

        if (key  == "A")
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce((transform.right * MoveSpeed) * -1);
            FacingRight = false;
        }
        if (key == "D")
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.right * MoveSpeed);
            FacingRight = true;
        }
        if (key == "W")
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce(transform.up * JumpPower);
            OnPlatform = false;
        }
        if (key == "S")
        {
            gameObject.GetComponent<Rigidbody2D>().AddForce((transform.up * DownPower) * -1);
        }
    }

    void AttackingFunction()
    {
        if (Input.GetKeyDown(KeyCode.G) && !AttackingOut && !AttackingIn && FacingRight && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) //check for key press and they are not already attacking
        {
            AttackingOut = true;
            AttackingRight = true;
        }
        else if (Input.GetKeyDown(KeyCode.G) && !AttackingOut && !AttackingIn && !FacingRight && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            AttackingOut = true;
            AttackingLeft = true;
        }
        else if (Input.GetKeyDown(KeyCode.G) && !AttackingOut && !AttackingIn && Input.GetKey(KeyCode.W))
        {
            AttackingOut = true;
            AttackingUp = true;
        }
        else if (Input.GetKeyDown(KeyCode.G) && !AttackingOut && !AttackingIn && Input.GetKey(KeyCode.S))
        {
            AttackingOut = true;
            AttackingDown = true;
        }
        AttackRight();
        AttackLeft();
        AttackUp();
        AttackDown();
    }

    void AttackingFunctionPlayer2()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1) && !AttackingOut && !AttackingIn && FacingRight && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)) //check for key press and they are not already attacking
        {
            AttackingOut = true;
            AttackingRight = true;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1) && !AttackingOut && !AttackingIn && !FacingRight && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
            AttackingOut = true;
            AttackingLeft = true;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1) && !AttackingOut && !AttackingIn && Input.GetKey(KeyCode.UpArrow))
        {
            AttackingOut = true;
            AttackingUp = true;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad1) && !AttackingOut && !AttackingIn && Input.GetKey(KeyCode.DownArrow))
        {
            AttackingOut = true;
            AttackingDown = true;
        }
        AttackRight();
        AttackLeft();
        AttackUp();
        AttackDown();
    }

    void AttackRight()
    {
        if (AttackingRight)
        {
            if (AttackingOut && Arm.transform.localPosition != new Vector3(1, 0)) //if they attacking and we have not reached the outward poition
            {
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(1, 0), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(1, 0) & !AttackingIn) //once we reach the outward position we change from going out to in
            {
                AttackingOut = false;
                AttackingIn = true;
            }
            else if (AttackingIn && Arm.transform.localPosition != new Vector3(0, 0)) //if we attacking in and have not reached inward poition
            {
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(0, 0), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(0, 0)) //we have reached inward position, so reset attack in
            {
                AttackingIn = false;
                AttackingRight = false;
            } 
        }
    }

    void AttackLeft()
    {
        if (AttackingLeft)
        {
            if (AttackingOut && Arm.transform.localPosition != new Vector3(-1, 0)) //if they attacking and we have not reached the outward poition
            {
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(-1, 0), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(-1, 0) & !AttackingIn) //once we reach the outward position we change from going out to in
            {
                AttackingOut = false;
                AttackingIn = true;
            }
            else if (AttackingIn && Arm.transform.localPosition != new Vector3(0, 0)) //if we attacking in and have not reached inward poition
            {
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(0, 0), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(0, 0)) //we have reached inward position, so reset attack in
            {
                AttackingIn = false;
                AttackingLeft = false;
            }
        }
    }

    void AttackUp()
    {
        if (AttackingUp)
        {
            if (AttackingOut && Arm.transform.localPosition != new Vector3(0, 1)) //if they attacking and we have not reached the outward poition
            {
                Arm.transform.localRotation = Quaternion.Euler(0, 0, 90);
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(0, 1), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(0, 1) & !AttackingIn) //once we reach the outward position we change from going out to in
            {
                AttackingOut = false;
                AttackingIn = true;
            }
            else if (AttackingIn && Arm.transform.localPosition != new Vector3(0, 0)) //if we attacking in and have not reached inward poition
            {
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(0, 0), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(0, 0)) //we have reached inward position, so reset attack in
            {
                AttackingIn = false;
                AttackingUp = false;
                Arm.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            
        }
    }

    void AttackDown()
    {
        if (AttackingDown)
        {
            
            if (AttackingOut && Arm.transform.localPosition != new Vector3(0, -1)) //if they attacking and we have not reached the outward poition
            {
                Arm.transform.localRotation = Quaternion.Euler(0, 0, 90);
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(0, -1), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(0, -1) & !AttackingIn) //once we reach the outward position we change from going out to in
            {
                AttackingOut = false;
                AttackingIn = true;
            }
            else if (AttackingIn && Arm.transform.localPosition != new Vector3(0, 0)) //if we attacking in and have not reached inward poition
            {
                Arm.transform.localPosition = Vector2.MoveTowards(Arm.transform.localPosition, new Vector2(0, 0), AttackSpeed);
            }
            else if (Arm.transform.localPosition == new Vector3(0, 0)) //we have reached inward position, so reset attack in
            {
                AttackingIn = false;
                AttackingDown = false;
                Arm.transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}
