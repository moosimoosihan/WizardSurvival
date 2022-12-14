using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{    
    //총알 관련 함수
    public float maxShootDelay;
    public float realDelay;
    public float curShootDelay;
    public float bulletSpeed;
    public int[] weaponLevel;
    public GameObject targetEnemy;
    int weapon2Count; // 돌맹이 회전 쿨타임 수
    float weapon4Count; // 따발총 쿨타임 수
    float weapon4Delay = 1f; // 따발총 쿨타임
    float weapon4MaxCount; // 띠발총 최대 쿨타임
    int weapon6Count; // 윈드 포스 쿨타임
    public GameObject[] weapon2LV;
    public GameObject weapon9Obj; // 물 오로라 오브젝트
    public GameObject thronArmor; // 가시 갑옷 오브젝트
    public int criticalChance = 1; //치명타 확률 (0~100)
    public float criticalDamage = 1.5f; //치명타 데미지
    public bool isCritical;
    int weaponHunter2Count; // 헌터 2초 딜레이
    public GameObject weaponWindForceMax; // 태풍 소환
    int weaponCrazyFireCount; // 미친 불 카운트
    int tsunamiCount; // 쓰나미 카운트
    int weaponMeteoCount; // 메테오 카운트
    float shieldCount = 0; // 쉴드 카운트
    bool isShield; // 쉴드가 현재 생성되어 있는지 여부
    public GameObject shieldObj; // 쉴드 이미지
    float hammerCount; // 돌아가는 해머 카운트
    float horseCount; // 말 달리는 카운트
    public GameObject horseAttackObj; // 말 타있는 오브젝트
    int crossAttackCount; //십자가 공격 카운트 
    public GameObject lightBeam; // 빔 오브젝트
    int energyForceCount; // 투사체 카운트
    int randomThrowCount; // 랜덤하게 던지는 무기 카운트
    int trapCount; // 지뢰 카운트
    public GameObject turretObj; // 터렛 오브젝트
    int bowCount; // 활 카운트
    public int brearCount; // 곰 카운트
    public GameObject bearObj; // 곰 오브젝트
    public int skullCount; // 스켈레톤 개수
    int skullCreateCount; // 스켈레톤 카운트
    int birdAttackCount; // 새 카운트
    public GameObject birdObj; // 새 오브젝트
    public bool snake; // 뱀 활성화 확인 함수
    public int snakeCount; // 뱀 카운트
    public GameObject golemObj; // 골렘 오브젝트
    public bool isGolem; // 골렘 활성화 확인 함수
    public int golemCount; // 골렘 카운트
    public int lakeCount; // 호수 카운트
    public bool isLake; // 호수 활성화 확인 함수
    float healingCount;

    //패시브 관련 함수
    public bool[,] passiveLevel; // 딜레이, 스피드, 체력, 파워, 자석
    public float power = 1f;
    float magScale = 1f;
    float passiveShootDelay = 1f;
    float passiveHealingCount; //따로 체력 10초 세어서 회복
    float passiveHealingTimeCount; // 체력 1초 간격

    //이동 관련 함수
    public Vector2 inputVec;
    public float speed;
    public bool joyStickOn;
    public float exp;
    public float life;
    public float maxLife;
    public int gold;
    public int enemyClearNum;

    public GameManager gameManager;
    public ObjectManager objectManager;

    public Camera cam;
    
    //플레이어가 죽은 상태를 나타내는 값
    public bool playerDead = false;
    public int playerDeadCount = 0;

    Rigidbody2D rigid;
    public Animator anim;
    public RuntimeAnimatorController[] animCharacter;
    SpriteRenderer spriteRenderer;

    //자석 함수
    public GameObject mag;

    //체력 회복량
    public float healthValue;

    //폭탄 이펙트
    public GameObject boomEffect;

    // 타겟 확인 이미지
    public GameObject targetImage;

    // 플레이어를 따라다니는 오브젝트 모음
    public GameObject playerFollowObj;

    void Start()
    {
        //모든 무기 레벨을 0으로 초기화
        for(int i=0;i<weaponLevel.Length;i++){
            weaponLevel[i] = 0;
        }
        passiveLevel = new bool[7,5];
        power = 1;
        gold = 0;
        switch(gameManager.character){
            case "Wizard":
                anim.runtimeAnimatorController = animCharacter[0];
            break;
            case "Hunter":
                // 모든 체력회복 2배
                anim.runtimeAnimatorController = animCharacter[1];
            break;
            case "Hunter2":
                anim.runtimeAnimatorController = animCharacter[2];
                life *= 2;
            break;
            case "Hunter3":
                anim.runtimeAnimatorController = animCharacter[3];
                speed += 0.2f;
            break;
            case "Hunter4":
                anim.runtimeAnimatorController = animCharacter[4];
                power += 1f;
            break;
        }
        maxLife=life;
    }
    void Awake()
    {
        //초기화
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerDeadCount = 0;
    }

    void Update()
    {
        //체력이 0이면 죽는 에니메이션
        if(life <= 0 && playerDeadCount == 0 && !playerDead){
            life = 0;
            playerDead = true;
            ReturnSprite();
            //죽으면 몬스터랑 만나지 않는 레이어로 바꿔줘
            gameObject.layer = 7;
            //죽음 에니메이션
            anim.SetBool("Dead",playerDead);
            //멈춰라
            Invoke("TimeStopChance",0.5f);
        } else if(life <= 0 && playerDeadCount == 1 && !playerDead) {
            life = 0;
            playerDead = true;
            ReturnSprite();
            //죽으면 몬스터랑 만나지 않는 레이어로 바꿔줘
            gameObject.layer = 7;
            //죽음 에니메이션
            anim.SetTrigger("Dead");
            //멈춰라
            Invoke("TimeStopDead",0.5f);
            gameManager.playerDead();
        } else {
            if(targetEnemy!=null && targetEnemy.activeSelf && gameManager.IsTargetVisible(cam, targetEnemy.transform)){
                targetImage.SetActive(true);
                targetImage.transform.position = targetEnemy.transform.position;
            } else {
                targetImage.SetActive(false);
            }
            playerFollowObj.transform.position = transform.position;

            Fire();
            Passive();
            Weapon4Fire();
            Reload();
            PassiveHealing();
            Shield();
        }
    }
    void FixedUpdate()
    {
        if(playerDead)
            return;

        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }
    void LateUpdate()
    {
        if(playerDead)
            return;
        
    //     if(joyStickOn)
    //         return;
            
         // 걷는 에니메이션(magnitude 순수 크기)
        anim.SetFloat("Speed", inputVec.magnitude);

        if(inputVec.x != 0 && !joyStickOn){
        // 캐릭터 좌, 우 방향전환
        spriteRenderer.flipX = inputVec.x < 0;
        }
    }

    // //인풋 시스템으로 이동 함수 단축
    // void OnMove(InputValue value)
    // {
    //     if(playerDead)
    //         return;

    //     inputVec = value.Get<Vector2>();
    // }

    void OnCollisionStay2D(Collision2D other) {
        //적군과 맞닿아 있다면 데미지가 들어가라
        if(playerDead) return;
        
        if(other.gameObject.tag == "Enemy"){
            if(isShield){
                if(shieldCount>=0.5f){
                    isShield = false;
                    shieldCount = 0;
                } else {
                    shieldCount += Time.deltaTime;
                }
            } else {
                EnemyMove enemy = other.gameObject.GetComponent<EnemyMove>();
                PlayerDamaged(enemy.power);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D other) {
        if(playerDead) return;

        // 호수에 닿아 있다면 회복
        if(other.gameObject.tag == "PlayerObj"){
            PlayerObj lakeLogic = other.gameObject.GetComponent<PlayerObj>();
            if(lakeLogic.type == "Lake"){
                healingCount += Time.deltaTime;
                if(healingCount>=5){
                    Healing(lakeLogic.power*0.05f);
                    healingCount = 0;
                }
            }
        }
    }
    void OnCollisionExit2D(Collision2D other) {
        //적군과 떨어지면 색상을 복구해라
        if(other.gameObject.tag == "Enemy"){
            ReturnSprite();
        }
    }
    void PlayerRed()
    {
        spriteRenderer.color = new Color(1,0,0);
        Invoke("ReturnSprite",0.2f);
    }
    //플레이어 데미지
    public void PlayerDamaged(float dmg)
    {
        spriteRenderer.color = new Color(1,0,0);
        //해당 적군을 찾아서 해당 적군의 데미지가 초당 들어가라
        life -= dmg * Time.deltaTime;
    }
    //플레이어 화염 데미지
    //한번에 구현 할 필요 있음!

    void OnCollisionEnter2D(Collision2D other) {
         if (other.gameObject.tag == "Enemy") {
            //적군과 맞닿았다면 데미지가 들어가라
            if(playerDead)
                return;

            if(isShield){
                if(shieldCount>=0.5f){
                    isShield = false;
                    shieldCount = 0;
                } else {
                    shieldCount += 0.1f;
                }
            } else {
                EnemyMove enemy = other.gameObject.GetComponent<EnemyMove>();
                life -= enemy.power;
            }
         }
    }
    void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.tag == "EnemyBullet"){
            if(isShield){ // 총알에 맞으면 쉴드가 바로 없어짐
                isShield = false;
                shieldCount = 0;
                other.gameObject.SetActive(false);
            } else {
                Bullet bullet = other.gameObject.GetComponent<Bullet>();
                life -= bullet.power;
                PlayerRed();
                other.gameObject.SetActive(false);
            }
            
        } else if(other.gameObject.tag == "Item"){
            Item item = other.gameObject.GetComponent<Item>();
            switch(item.type){
                //체력 회복
                case "Health":
                    Healing(healthValue);
                break;
                //자석
                case "Mag":
                    GameObject[] itemExp0 = objectManager.GetPool("ItemExp0");
                    for(int i=0;i<itemExp0.Length;i++){
                        Item itemExp0Logic = itemExp0[i].GetComponent<Item>();
                        itemExp0Logic.isMagnetOn = true;
                    }
                    GameObject[] itemExp1 = objectManager.GetPool("ItemExp1");
                    for(int i=0;i<itemExp1.Length;i++){
                        Item itemExp1Logic = itemExp1[i].GetComponent<Item>();
                        itemExp1Logic.isMagnetOn = true;
                    }
                    GameObject[] itemExp2 = objectManager.GetPool("ItemExp2");
                    for(int i=0;i<itemExp2.Length;i++){
                        Item itemExp2Logic = itemExp2[i].GetComponent<Item>();
                        itemExp2Logic.isMagnetOn = true;
                    }
                break;
                //경험치 10
                case "Exp0":
                    exp += 10;
                break;
                //경험치 50
                case "Exp1":
                    exp += 50;
                break;
                //경험치 100
                case "Exp2":
                    exp += 100;
                break;
                //동전0
                case "Coin0":
                    gold += 10;
                break;
                //동전1
                case "Coin1":
                    gold += 50;
                break;
                //동전2
                case "Coin2":
                    gold += 100;
                break;

                //폭탄 아이템
                case "Boom":
                    Boom(1000, true);
                break;
                case "Box":
                    // 박스를 먹을 경우 다른걸로 대체
                break;
                case "Shield":
                    if(isShield){ // 쉴드가 있다면 쉴드를 다시 0초로
                        shieldCount = 0f;
                    } else { // 없다면 쉴드 생성하고 0초로
                        shieldCount = 0f;
                        isShield = true;
                    }
                break;
            }
            other.gameObject.SetActive(false);
        }
    }
    void ReturnSprite()
    {
        spriteRenderer.color = new Color(1,1,1);
    }

    //미사일이 발사되는 함수
    void Fire()
    {
        if(curShootDelay < realDelay || !targetImage.activeSelf){
            return;
        }

        //무기
        if(gameManager.character=="Wizard"){
        // 아이스 스피어 (데미지 증가 스케일 업)
        switch(weaponLevel[0]){
            case 1:
                WeaponIceSpear(50, 1f,false);
            break;
            case 2:
                WeaponIceSpear(100, 1.2f,false);
            break;
            case 3:
                WeaponIceSpear(150, 1.3f,false);
            break;
            case 4:
                WeaponIceSpear(200, 1.4f,false);
            break;
            case 5:
                WeaponIceSpear(250, 1.5f,false);
            break;
            case 6:
                WeaponIceSpear(500, 1.8f,true);
            break;
            }
            //윈드포스
            switch(weaponLevel[1]){
            case 1:
                if(weapon6Count<4){
                    weapon6Count++;
                } else {
                    WeaponWindForce(30,1f,1f);
                }
            break;
            case 2:
                if(weapon6Count<4){
                    weapon6Count++;
                } else {
                    WeaponWindForce(60,1.1f,1.2f);
                }
            break;
            case 3:
                if(weapon6Count<4){
                    weapon6Count++;
                } else {
                    WeaponWindForce(90,1.2f,1.5f);
                }
            break;
            case 4:
                if(weapon6Count<4){
                    weapon6Count++;
                } else {
                    WeaponWindForce(120,1.3f,1.7f);
                }
            break;
            case 5:
                if(weapon6Count<4){
                    weapon6Count++;
                } else {
                    WeaponWindForce(160,1.5f,2f);
                }
            break;
            case 6:
                if(weapon6Count<4){
                    weapon6Count++;
                } else {
                    WeaponWindForce(200,1.5f,2f);
                }
                if(!weaponWindForceMax.activeSelf){
                    weaponWindForceMax.SetActive(true);
                }
                
            break;
            }
            switch(weaponLevel[2]){//파이어 볼
            case 1:
                WeaponFireBall(50f, 1f, false);
            break;
            case 2:
                WeaponFireBall(80f, 1.2f, false);
            break;
            case 3:
                WeaponFireBall(120f, 1.4f, false);
            break;
            case 4:
                WeaponFireBall(150f, 1.6f, false);
            break;
            case 5:
                WeaponFireBall(190f, 1.8f, false);
            break;
            case 6:
                WeaponFireBall(250f, 2f, true);
            break;
            }
            switch(weaponLevel[3]){// 체인 라이트닝
            case 1:
                WeaponChainLightning(40, 2,false);
            break;
            case 2:
                WeaponChainLightning(80, 3,false);
            break;
            case 3:
                WeaponChainLightning(120, 4,false);
            break;
            case 4:
                WeaponChainLightning(160, 5,false);
            break;
            case 5:
                WeaponChainLightning(200, 6,false);
            break;
            case 6:
                WeaponChainLightning(300, 6,true);
            break;
            }
            switch(weaponLevel[4]){//물 오로라
            case 1:
                weapon9Obj.SetActive(true);
            break;
            case 2:
                weapon9Obj.SetActive(true);
                weapon9Obj.transform.localScale = new Vector3(0.65f,0.65f,0);
            break;
            case 3:
                weapon9Obj.SetActive(true);
                weapon9Obj.transform.localScale = new Vector3(0.8f,0.8f,0);
            break;
            case 4:
                weapon9Obj.SetActive(true);
                weapon9Obj.transform.localScale = new Vector3(0.95f,0.95f,0);
            break;
            case 5:
                weapon9Obj.SetActive(true);
                weapon9Obj.transform.localScale = new Vector3(1.1f,1.1f,0);
            break;
            case 6:
                weapon9Obj.SetActive(true);
                weapon9Obj.transform.localScale = new Vector3(1.2f,1.2f,0);
                if(tsunamiCount>2){
                    WeaponTsunami(500);
                } else {
                    tsunamiCount++;
                }
            break;
            }
            //돌아가는 돌맹이
            switch(weaponLevel[5]){
                case 1:
                    if(weapon2Count<=0){
                        if(!weapon2LV[0].activeSelf){
                            weapon2LV[0].SetActive(true);
                            weapon2LV[1].SetActive(true);
                            weapon2Count = 10;
                        }
                    } else {
                        weapon2Count--;
                    }
                break;
                case 2:
                    if(weapon2Count<=0){
                        if(weapon2LV[0].activeSelf){
                            weapon2LV[0].SetActive(false);
                            weapon2LV[1].SetActive(false);
                        }
                        if(!weapon2LV[2].activeSelf){
                            weapon2LV[2].SetActive(true);
                            weapon2LV[3].SetActive(true);
                            weapon2LV[4].SetActive(true);
                            weapon2Count = 9;
                        }
                    } else {
                        weapon2Count--;
                    }   
                break;
                case 3:
                    if(weapon2Count<=0){
                        if(weapon2LV[2].activeSelf){
                            weapon2LV[2].SetActive(false);
                            weapon2LV[3].SetActive(false);
                            weapon2LV[4].SetActive(false);
                        }
                        if(!weapon2LV[5].activeSelf){
                            weapon2LV[5].SetActive(true);
                            weapon2LV[6].SetActive(true);
                            weapon2LV[7].SetActive(true);
                            weapon2LV[8].SetActive(true);
                            weapon2Count = 8;
                        }
                    } else {
                        weapon2Count--;
                    }
                break;
                case 4:
                    if(weapon2Count<=0){
                        if(weapon2LV[5].activeSelf){
                            weapon2LV[5].SetActive(false);
                            weapon2LV[6].SetActive(false);
                            weapon2LV[7].SetActive(false);
                            weapon2LV[8].SetActive(false);
                        }
                        if(!weapon2LV[9].activeSelf){
                            weapon2LV[9].SetActive(true);
                            weapon2LV[10].SetActive(true);
                            weapon2LV[11].SetActive(true);
                            weapon2LV[12].SetActive(true);
                            weapon2LV[13].SetActive(true);
                            weapon2Count = 7;
                        }
                    } else {
                        weapon2Count--;
                    }   
                break;
                case 5:
                    if(weapon2Count<=0){
                        if(weapon2LV[9].activeSelf){
                            weapon2LV[9].SetActive(false);
                            weapon2LV[10].SetActive(false);
                            weapon2LV[11].SetActive(false);
                            weapon2LV[12].SetActive(false);
                            weapon2LV[13].SetActive(false);
                        }
                        if(!weapon2LV[14].activeSelf){
                            weapon2LV[14].SetActive(true);
                            weapon2LV[15].SetActive(true);
                            weapon2LV[16].SetActive(true);
                            weapon2LV[17].SetActive(true);
                            weapon2LV[18].SetActive(true);
                            weapon2LV[19].SetActive(true);
                            weapon2Count = 6;
                        }
                    } else {
                        weapon2Count--;
                    }
                break;
                case 6:
                    if(weapon2Count<=0){
                        if(weapon2LV[9].activeSelf){
                            weapon2LV[9].SetActive(false);
                            weapon2LV[10].SetActive(false);
                            weapon2LV[11].SetActive(false);
                            weapon2LV[12].SetActive(false);
                            weapon2LV[13].SetActive(false);
                        }
                        if(!weapon2LV[14].activeSelf){
                            weapon2LV[14].SetActive(true);
                            weapon2LV[15].SetActive(true);
                            weapon2LV[16].SetActive(true);
                            weapon2LV[17].SetActive(true);
                            weapon2LV[18].SetActive(true);
                            weapon2LV[19].SetActive(true);
                            weapon2Count = 6;
                        }
                    } else {
                        weapon2Count--;
                    }
                    if(weaponMeteoCount<=0){
                        WeaponMeteo(1000);
                        weaponMeteoCount=10;
                    } else {
                        weaponMeteoCount--;
                    }
                break;
            }
        } else if(gameManager.character == "Hunter"){
                if(weaponHunter2Count>1){
                    //곡괭이
                    switch(weaponLevel[0]){
                        case 1:
                            WeaponPickaxe(120,1,false);
                        break;
                        case 2:
                            WeaponPickaxe(200,2,false);
                        break;
                        case 3:
                            WeaponPickaxe(300,3,false);
                        break;
                        case 4:
                            WeaponPickaxe(400,4,false);
                        break;
                        case 5:
                            WeaponPickaxe(600,5,false);
                        break;
                        case 6:
                            WeaponPickaxe(800,5,true);
                        break;
                    }
                    //아래로 던지는 관통 삽
                    switch(weaponLevel[1]){
                        case 1:
                            WeaponShovel(100,1);
                        break;
                        case 2:
                            WeaponShovel(200,2);
                        break;
                        case 3:
                            WeaponShovel(300,3);
                        break; 
                        case 4:
                            WeaponShovel(400,4);
                        break;
                        case 5:
                            WeaponShovel(500,5);
                        break;
                        case 6:
                            WeaponShovel(600,10);
                        break;
                        }
                    //관통 창 무기
                    switch(weaponLevel[2]){
                        case 1:
                            WeaponTrident(80,0.5f,false);
                        break;
                        case 2:                
                            WeaponTrident(160,0.7f,false);
                        break;
                        case 3:                
                            WeaponTrident(240,0.9f,false);
                        break;
                        case 4:                
                            WeaponTrident(320,1.4f,false);
                        break;
                        case 5:                
                            WeaponTrident(400,1.8f,false);
                        break;
                        case 6:                
                            WeaponTrident(500,1.8f,true);
                        break;
                    }
                    weaponHunter2Count = 0;
                } else {
                        weaponHunter2Count++;
                    }
                //샷건 (탄 증가)
                switch(weaponLevel[4]){
                case 1:
                    WeaponShotgun(100,2,false);
                break;
                case 2:
                    WeaponShotgun(200,3,false);
                break;
                case 3:
                    WeaponShotgun(300,4,false);
                break;
                case 4:
                    WeaponShotgun(400,5,false);
                break;
                case 5:                
                    WeaponShotgun(500,6,false);
                break;
                case 6:                
                    WeaponShotgun(600,24,true);
                break;
                }
                //권총
                switch(weaponLevel[5]){
                    case 1:
                        WeaponPistol(100,0.5f);
                    break;
                    case 2:
                        WeaponPistol(200,0.5f);
                    break;
                    case 3:
                        WeaponPistol(300,0.5f);
                    break;
                    case 4:
                        WeaponPistol(400,0.5f);
                    break;
                    case 5:
                        WeaponPistol(500,0.5f);
                    break;
                    case 6:
                        WeaponPistol(1000,3f);
                    break;
                }
            } else if(gameManager.character == "Hunter2"){ //헌터 2 성기사 느낌
                switch(weaponLevel[0]){ // 가까이 있는 경우 데미지
                    case 0:
                        if(thronArmor.activeSelf){
                            thronArmor.SetActive(false);
                        }
                    break;
                    default : 
                        if(!thronArmor.activeSelf){
                            thronArmor.SetActive(true);
                        }
                    break;
                    case 6:
                        // 가시갑옷 궁극기
                        Bullet bullet = thronArmor.GetComponent<Bullet>();
                        if(!bullet.armorKill){
                            bullet.armorKill=true;
                        }
                    break;
                }
                switch(weaponLevel[1]){ // 돌아가는 망치
                    case 1:
                        if(hammerCount<=0){
                            ThrowHammer(50,false);
                            hammerCount = 2;
                        } else {
                            hammerCount--;
                        }
                    break;
                    case 2:
                        if(hammerCount<=0){
                            ThrowHammer(80,false);
                            hammerCount = 2;
                        } else {
                            hammerCount--;
                        }
                    break;
                    case 3:
                        if(hammerCount<=0){
                            ThrowHammer(120,false);
                            hammerCount = 2;
                        } else {
                            hammerCount--;
                        }
                    break;
                    case 4:
                        if(hammerCount<=0){
                            ThrowHammer(150,false);
                            hammerCount = 2;
                        } else {
                            hammerCount--;
                        }
                    break;
                    case 5:
                        if(hammerCount<=0){
                            ThrowHammer(180,false);
                            hammerCount = 0;
                        } else {
                            hammerCount--;
                        }
                    break;
                    case 6:
                        if(hammerCount<=0){
                            ThrowHammer(250,true);
                            hammerCount = 0;
                        } else {
                            hammerCount--;
                        }
                    break;
                }
                switch(weaponLevel[2]){ // 메이스 공격
                    case 1:
                        MaceAttack(80,50,false);
                    break;
                    case 2:
                        MaceAttack(160,100,false);
                    break;
                    case 3:
                        MaceAttack(240,150,false);
                    break;
                    case 4:
                        MaceAttack(320,200,false);
                    break;
                    case 5:
                        MaceAttack(400,250,false);
                    break;
                    case 6:
                        MaceAttack(500,250,true);
                    break;
                }
                switch(weaponLevel[3]){ // 5초마다 빠르게 전진하며 적군을 밀쳐내며 공격
                    case 1:
                        //2초 빠르게 달리고 5초 쉬고
                        if(horseCount<=0){
                            horseCount = 7;
                            HorseAttack(50,false);
                        } else {
                            horseCount--;
                        }
                    break;
                    case 2:
                        if(horseCount<=0){
                            horseCount = 7;
                            HorseAttack(100,false);
                        } else {
                            horseCount--;
                        }
                    break;
                    case 3:
                        if(horseCount<=0){
                            horseCount = 7;
                            HorseAttack(140,false);
                        } else {
                            horseCount--;
                        }
                    break;
                    case 4:
                        if(horseCount<=0){
                            horseCount = 7;
                            HorseAttack(200,false);
                        } else {
                            horseCount--;
                        }
                    break;
                    case 5:
                        if(horseCount<=0){
                            horseCount = 7;
                            HorseAttack(240,false);
                        } else {
                            horseCount--;
                        }
                    break;
                    case 6:
                        if(horseCount<=0){
                            horseCount = 7;
                            HorseAttack(300,true);
                        } else {
                            horseCount--;
                        }
                    break;
                }
                switch(weaponLevel[4]){ // 십자가 모양으로 공격
                    case 1:
                        if(crossAttackCount <= 0){
                            CrossAttack(40,false);
                            crossAttackCount=4;
                        } else {
                            crossAttackCount--;
                        }
                    break;
                    case 2:
                        if(crossAttackCount <= 0){
                            CrossAttack(80,false);
                            crossAttackCount=4;
                        } else {
                            crossAttackCount--;
                        }
                    break;
                    case 3:
                        if(crossAttackCount <= 0){
                            CrossAttack(120,false);
                            crossAttackCount=4;
                        } else {
                            crossAttackCount--;
                        }
                    break;
                    case 4:
                        if(crossAttackCount <= 0){
                            CrossAttack(160,false);
                            crossAttackCount=4;
                        } else {
                            crossAttackCount--;
                        }
                    break;
                    case 5:
                        if(crossAttackCount <= 0){
                            CrossAttack(200,false);
                            crossAttackCount=4;
                        } else {
                            crossAttackCount--;
                        }
                    break;
                    case 6:
                        if(crossAttackCount <= 0){
                            CrossAttack(250,true);
                            crossAttackCount=4;
                        } else {
                            crossAttackCount--;
                        }
                    break;
                }
                switch(weaponLevel[5]){ // 빛 줄기
                    case 1:
                        if(!lightBeam.activeSelf){
                            lightBeam.SetActive(true);
                        }
                        LightBeam(30, 2f);
                    break;
                    case 2:
                        if(!lightBeam.activeSelf){
                            lightBeam.SetActive(true);
                        }
                        LightBeam(60, 3f);
                    break;
                    case 3:
                        if(!lightBeam.activeSelf){
                            lightBeam.SetActive(true);
                        }
                        LightBeam(90, 5f);
                    break;
                    case 4:
                        if(!lightBeam.activeSelf){
                            lightBeam.SetActive(true);
                        }
                        LightBeam(120, 8f);
                    break;
                    case 5:
                        if(!lightBeam.activeSelf){
                            lightBeam.SetActive(true);
                        }
                        LightBeam(150, 10f);
                    break;
                    case 6:
                        if(!lightBeam.activeSelf){
                            lightBeam.SetActive(true);
                        }
                        LightBeam(400, 10f);
                    break;
                }
            } else if(gameManager.character == "Hunter3"){ //헌터 3 돌아다니는 물체
                switch(weaponLevel[0]){ // 1 파동
                    case 1:
                        if(energyForceCount <= 0){
                            EnergyForce(60,false);
                            energyForceCount = 5;
                        } else {
                            energyForceCount--;
                        }
                    break;
                    case 2:
                        if(energyForceCount <= 0){
                            EnergyForce(100,false);
                            energyForceCount = 5;
                        } else {
                            energyForceCount--;
                        }
                    break;
                    case 3:
                        if(energyForceCount <= 0){
                            EnergyForce(140,false);
                            energyForceCount = 5;
                        } else {
                            energyForceCount--;
                        }
                    break;
                    case 4:
                        if(energyForceCount <= 0){
                            EnergyForce(180,false);
                            energyForceCount = 5;
                        } else {
                            energyForceCount--;
                        }
                    break;
                    case 5:
                        if(energyForceCount <= 0){
                            EnergyForce(220,false);
                            energyForceCount = 5;
                        } else {
                            energyForceCount--;
                        }
                    break;
                    case 6:
                        if(energyForceCount <= 0){
                            EnergyForce(300,true);
                            energyForceCount = 1;
                        } else {
                            energyForceCount--;
                        }
                    break;
                }
                switch(weaponLevel[1]){ // 2 파동
                    case 1:
                        EnergyForce2(20,false);
                    break;
                    case 2:
                        EnergyForce2(40,false);
                    break;
                    case 3:
                        EnergyForce2(60,false);
                    break;
                    case 4:
                        EnergyForce2(80,false);
                    break;
                    case 5:
                        EnergyForce2(100,false);
                    break;
                    case 6:
                        EnergyForce2(150,true);
                    break;
                }
                switch(weaponLevel[2]){ // 랜덤 물체
                    case 1:
                        if(randomThrowCount<=0){
                            RandomThrow(1,false);
                            randomThrowCount = 3;
                        } else {
                            randomThrowCount--;
                        }
                    break;
                    case 2:
                        if(randomThrowCount<=0){
                            RandomThrow(2,false);
                            randomThrowCount = 3;
                        } else {
                            randomThrowCount--;
                        }
                    break;
                    case 3:
                        if(randomThrowCount<=0){
                            RandomThrow(3,false);
                            randomThrowCount = 3;
                        } else {
                            randomThrowCount--;
                        }
                    break;
                    case 4:
                        if(randomThrowCount<=0){
                            RandomThrow(4,false);
                            randomThrowCount = 3;
                        } else {
                            randomThrowCount--;
                        }
                    break;
                    case 5:
                        if(randomThrowCount<=0){
                            RandomThrow(5,false);
                            randomThrowCount = 3;
                        } else {
                            randomThrowCount--;
                        }
                    break;
                    case 6:
                        if(randomThrowCount<=0){
                            RandomThrow(6,true);
                            randomThrowCount = 3;
                        } else {
                            randomThrowCount--;
                        }
                    break;
                }
                switch(weaponLevel[3]){ // 지뢰
                    case 1:
                        if(trapCount<=0){
                            TrapCreat(80, 2f);
                            trapCount=2;
                        } else {
                            trapCount--;
                        }
                    break;
                    case 2:
                        if(trapCount<=0){
                            TrapCreat(160, 2.2f);
                            trapCount=2;
                        } else {
                            trapCount--;
                        }
                    break;
                    case 3:
                        if(trapCount<=0){
                            TrapCreat(240, 2.4f);
                            trapCount=2;
                        } else {
                            trapCount--;
                        }
                    break;
                    case 4:
                        if(trapCount<=0){
                            TrapCreat(320, 2.6f);
                            trapCount=2;
                        } else {
                            trapCount--;
                        }
                    break;
                    case 5:
                        if(trapCount<=0){
                            TrapCreat(400, 2.8f);
                            trapCount=2;
                        } else {
                            trapCount--;
                        }
                    break;
                    case 6:
                        TrapCreat(480, 3f);
                    break;
                }
                switch(weaponLevel[4]){ // 포탑 설치
                    case 1:
                        TurretCreat(50,2f,false);
                    break;
                    case 2:
                        TurretCreat(100,2.2f,false);
                    break;
                    case 3:
                        TurretCreat(150,2.4f,false);
                    break;
                    case 4:
                        TurretCreat(200,2.6f,false);
                    break;
                    case 5:
                        TurretCreat(250,2.8f,false);
                    break;
                    case 6:
                        TurretCreat(300,3f,true);
                    break;
                }
                switch(weaponLevel[5]){ // 활
                    case 1:
                        if(bowCount<=0){
                            BowThrow(50,1,false);
                            bowCount = 1;
                        } else {
                            bowCount--;
                        }
                    break;
                    case 2:
                        if(bowCount<=0){
                            BowThrow(80,2,false);
                            bowCount = 1;
                        } else {
                            bowCount--;
                        }
                    break;
                    case 3:
                        if(bowCount<=0){
                            BowThrow(120,3,false);
                            bowCount = 1;
                        } else {
                            bowCount--;
                        }
                    break;
                    case 4:
                        if(bowCount<=0){
                            BowThrow(150,4,false);
                            bowCount = 1;
                        } else {
                            bowCount--;
                        }
                    break;
                    case 5:
                        if(bowCount<=0){
                            BowThrow(200,5,false);
                            bowCount = 1;
                        } else {
                            bowCount--;
                        }
                    break;
                    case 6:
                        BowThrow(250,10,true);
                    break;
                }
            } else if(gameManager.character == "Hunter4"){ // 헌터 4 소환수
                switch(weaponLevel[0]){ // 느린 공격 데미지 높은 곰
                    case 1:
                        if(brearCount<=0){
                            Bear(60, 600, false);
                            brearCount=1;
                        } else {
                            brearCount--;
                        }
                    break;
                    case 2:
                        if(brearCount<=0){
                            Bear(120, 1200, false);
                            brearCount=1;
                        } else {
                            brearCount--;
                        }
                    break;
                    case 3:
                        if(brearCount<=0){
                            Bear(180, 1800, false);
                            brearCount=1;
                        } else {
                            brearCount--;
                        }
                    break;
                    case 4:
                        if(brearCount<=0){
                            Bear(240, 2400, false);
                            brearCount=1;
                        } else {
                            brearCount--;
                        }
                    break;
                    case 5:
                        if(brearCount<=0){
                            Bear(300, 3000, false);
                            brearCount=1;
                        } else {
                            brearCount--;
                        }
                    break;
                    case 6:
                        Bear(360, 5000, true);
                    break;
                }
                switch(weaponLevel[1]){ // 스켈레톤
                    case 1:
                        if(skullCreateCount<=0){
                            SkullCreat(20,400,1,false);
                            skullCreateCount = 1;
                        } else {
                            skullCreateCount--;
                        }
                    break;
                    case 2:
                        if(skullCreateCount<=0){
                            SkullCreat(40,800,2,false);
                            skullCreateCount = 1;
                        } else {
                            skullCreateCount--;
                        }
                    break;
                    case 3:
                        if(skullCreateCount<=0){
                            SkullCreat(80,1600,3,false);
                            skullCreateCount = 1;
                        } else {
                            skullCreateCount--;
                        }
                    break;
                    case 4:
                        if(skullCreateCount<=0){
                            SkullCreat(100,2400,4,false);
                            skullCreateCount = 1;
                        } else {
                            skullCreateCount--;
                        }
                    break;
                    case 5:
                        if(skullCreateCount<=0){
                            SkullCreat(120,3200,5,false);
                            skullCreateCount = 1;
                        } else {
                            skullCreateCount--;
                        }
                    break;
                    case 6:
                        SkullCreat(150,4000,5,true);
                    break;
                }
                switch(weaponLevel[2]){ // 적군을 쫒아 공격하는 새
                    case 1:
                        if(birdAttackCount<=0){
                            BirdCreat(80,0,false);
                            birdAttackCount = 2;
                        } else {
                            birdAttackCount--;
                        }
                    break;
                    case 2:
                        if(birdAttackCount<=0){
                            BirdCreat(120,1,false);
                            birdAttackCount = 2;
                        } else {
                            birdAttackCount--;
                        }
                    break;
                    case 3:
                        if(birdAttackCount<=0){
                            BirdCreat(150,2,false);
                            birdAttackCount = 2;
                        } else {
                            birdAttackCount--;
                        }
                    break;
                    case 4:
                        if(birdAttackCount<=0){
                            BirdCreat(180,3,false);
                            birdAttackCount = 2;
                        } else {
                            birdAttackCount--;
                        }
                    break;
                    case 5:
                        if(birdAttackCount<=0){
                            BirdCreat(210,4,false);
                            birdAttackCount = 2;
                        } else {
                            birdAttackCount--;
                        }
                    break;
                    case 6:
                        if(birdAttackCount<=0){
                            BirdCreat(250,5,true);
                            birdAttackCount = 2;
                        } else {
                            birdAttackCount--;
                        }
                    break;
                }
                switch(weaponLevel[3]){ // 독을 뿜는 뱀
                    case 1:
                        if(snakeCount<=0){
                            SnakeCreat(30, 0.8f, false);
                        } else {
                            snakeCount--;
                        }
                    break;
                    case 2:
                        if(snakeCount<=0){
                            SnakeCreat(60, 1f, false);
                        } else {
                            snakeCount--;
                        }
                    break;
                    case 3:
                        if(snakeCount<=0){
                            SnakeCreat(90, 1.2f, false);
                        } else {
                            snakeCount--;
                        }
                    break;
                    case 4:
                        if(snakeCount<=0){
                            SnakeCreat(120, 1.4f, false);
                        } else {
                            snakeCount--;
                        }
                    break;
                    case 5:
                        if(snakeCount<=0){
                            SnakeCreat(150, 1.6f, false);
                        } else {
                            snakeCount--;
                        }
                    break;
                    case 6:
                        if(snakeCount<=0){
                            SnakeCreat(180, 1.8f, true);
                        } else {
                            snakeCount--;
                        }
                    break;
                }
                switch(weaponLevel[4]){ // 골램
                    case 1:
                        if(golemCount<=0){
                            Golem(40,500,false);
                        } else {
                            golemCount--;
                        }
                    break;
                    case 2:
                        if(golemCount<=0){
                            Golem(80,1000,false);
                        } else {
                            golemCount--;
                        }
                    break;
                    case 3:
                        if(golemCount<=0){
                            Golem(120,1500,false);
                        } else {
                            golemCount--;
                        }
                    break;
                    case 4:
                        if(golemCount<=0){
                            Golem(160,2000,false);
                        } else {
                            golemCount--;
                        }
                    break;
                    case 5:
                        if(golemCount<=0){
                            Golem(200,2500,false);
                        } else {
                            golemCount--;
                        }
                    break;
                    case 6:
                        if(golemCount<=0){
                            Golem(250,3100,true);
                        } else {
                            golemCount--;
                        }
                    break;
                }
                switch(weaponLevel[5]){ // 물 호수
                    case 1:
                        if(lakeCount<=0){
                            LakeCreate(30, false);
                        } else {
                            lakeCount--;
                        }
                    break;
                    case 2:
                        if(lakeCount<=0){
                            LakeCreate(50, false);
                        } else {
                            lakeCount--;
                        }
                    break;
                    case 3:
                        if(lakeCount<=0){
                            LakeCreate(70, false);
                        } else {
                            lakeCount--;
                        }
                    break;
                    case 4:
                        if(lakeCount<=0){
                            LakeCreate(90, false);
                        } else {
                            lakeCount--;
                        }
                    break;
                    case 5:
                        if(lakeCount<=0){
                            LakeCreate(110, false);
                        } else {
                            lakeCount--;
                        }
                    break;
                    case 6:
                        if(lakeCount<=0){
                            LakeCreate(200, true);
                        } else {
                            lakeCount--;
                        }
                    break;
                }
            }
        //재장전
        curShootDelay = 0;
    }
    void Weapon4Fire()
    {
        if(gameManager.character != "Hunter" || weapon4Count < weapon4MaxCount || !targetImage.activeSelf)
            return;

        //따발총(총알 딜레이 감소)
        switch(weaponLevel[3]){
            case 1:
                weapon4Delay = 2f;
                WeaponMachineGun(50);
            break;
            case 2:
                weapon4Delay = 3f;
                WeaponMachineGun(100);
            break;
            case 3:
                weapon4Delay = 4f;
                WeaponMachineGun(150);
            break;
            case 4:
                weapon4Delay = 5f;
                WeaponMachineGun(200);
            break;
            case 5:
                weapon4Delay = 6f;
                WeaponMachineGun(250);
            break;
            case 6:
                weapon4Delay = 10f;
                WeaponMachineGun(350);
            break;
        }
    }
    void PassiveHealing()
    {
        if(passiveHealingTimeCount>=1){
            switch(weaponLevel[8]){ // 5초에 한번 체력 회복
            case 1:
                if(passiveHealingCount > 5){
                    Healing(maxLife*0.01f);
                    passiveHealingCount = 0;
                } else {
                    passiveHealingCount++;
                }
            break;
            case 2:
                if(passiveHealingCount > 5){
                    Healing(maxLife*0.02f);
                    passiveHealingCount = 0;
                } else {
                    passiveHealingCount++;
                }
            break;
            case 3:
                if(passiveHealingCount > 5){
                    Healing(maxLife*0.03f);
                    passiveHealingCount = 0;
                } else {
                    passiveHealingCount++;
                }
            break;
            case 4:
                if(passiveHealingCount > 5){
                    Healing(maxLife*0.04f);
                    passiveHealingCount = 0;
                } else {
                    passiveHealingCount++;
                }
            break;
            case 5:
                if(passiveHealingCount > 5){
                    Healing(maxLife*0.05f);
                    passiveHealingCount = 0;
                } else {
                    passiveHealingCount++;
                }
            break;
            }
            passiveHealingTimeCount = 0;
        }
    }
    void Passive()
    {
        //패시브
        //쿨타임 감소
        switch(weaponLevel[6]){
            case 1:
            if(!passiveLevel[0,0]){
                passiveLevel[0,0] = true;
                passiveShootDelay -= (0.09f+0.01f);
            }
            break;
            case 2:
            if(!passiveLevel[0,1]){
                passiveLevel[0,1] = true;
                passiveShootDelay -= (0.09f+0.01f);
            }
            break;
            case 3:
            if(!passiveLevel[0,2]){
                passiveLevel[0,2] = true;
                passiveShootDelay -= (0.09f+0.01f);
            }
            break;
            case 4:
            if(!passiveLevel[0,3]){
                passiveLevel[0,3] = true;
                passiveShootDelay -= (0.09f+0.01f);
            }
            break;
            case 5:
            if(!passiveLevel[0,4]){
                passiveLevel[0,4] = true;
                passiveShootDelay -= (0.09f+0.01f);
            }
            break;

        }
        //스피드 업
        switch(weaponLevel[7]){
            case 1:
                if(!passiveLevel[1,0]){
                    passiveLevel[1,0] = true;
                    speed += 0.2f;
                }
            break;
            case 2:
                if(!passiveLevel[1,1]){
                    passiveLevel[1,1] = true;
                    speed += 0.2f;
                }
            break;
            case 3:
                if(!passiveLevel[1,2]){
                    passiveLevel[1,2] = true;
                    speed += 0.2f;
                }
            break;
            case 4:
                if(!passiveLevel[1,3]){
                    passiveLevel[1,3] = true;
                    speed += 0.2f;
                }
            break;
            case 5:
                if(!passiveLevel[1,4]){
                    passiveLevel[1,4] = true;
                    speed += 0.2f;
                }
            break;
        }
        //체력 업
        switch(weaponLevel[8]){
            case 1:
            if(!passiveLevel[2,0]){
                passiveLevel[2,0] = true;
                maxLife += 0.09f + 0.01f;
            }
            break;
            case 2:
            if(!passiveLevel[2,1]){
                passiveLevel[2,1] = true;
                maxLife += 0.09f + 0.01f;
            }
            break;
            case 3:
            if(!passiveLevel[2,2]){
                passiveLevel[2,2] = true;
                maxLife += 0.09f + 0.01f;
            }
            break;
            case 4:
            if(!passiveLevel[2,3]){
                passiveLevel[2,3] = true;
                maxLife += 0.09f + 0.01f;
            }
            break;
            case 5:
            if(!passiveLevel[2,4]){
                passiveLevel[2,4] = true;
                maxLife += 0.09f + 0.01f;
            }
            break;
        }
        //공격력 업
        switch(weaponLevel[9]){
            case 1:
                if(!passiveLevel[3,0]){
                    passiveLevel[3,0] = true;
                    power += 0.09f + 0.01f;
                }
            break;
            case 2:
                if(!passiveLevel[3,1]){
                    passiveLevel[3,1] = true;
                    power += 0.09f + 0.01f;
                }
            break;
            case 3:
                if(!passiveLevel[3,2]){
                    passiveLevel[3,2] = true;
                    power += 0.09f + 0.01f;
                }
            break;
            case 4:
                if(!passiveLevel[3,3]){
                    passiveLevel[3,3] = true;
                    power += 0.09f + 0.01f;
                }
            break;
            case 5:
                if(!passiveLevel[3,4]){
                    passiveLevel[3,4] = true;
                    power += 0.09f + 0.01f;
                }
            break;
        }
        //자석
        switch(weaponLevel[10]){
            case 1:
            if(!passiveLevel[4,0]){
                passiveLevel[4,0] = true;
                magScale += 0.9f;
                mag.transform.localScale = new Vector3(magScale,magScale,0);
            }
            break;
            case 2:
            if(!passiveLevel[4,1]){
                passiveLevel[4,1] = true;
                magScale += 0.9f;
                mag.transform.localScale = new Vector3(magScale,magScale,0);
            }
            break;
            case 3:
            if(!passiveLevel[4,2]){
                passiveLevel[4,2] = true;
                magScale += 0.9f;
                mag.transform.localScale = new Vector3(magScale,magScale,0);
            }
            break;
            case 4:
            if(!passiveLevel[4,3]){
                passiveLevel[4,3] = true;
                magScale += 0.9f;
                mag.transform.localScale = new Vector3(magScale,magScale,0);
            }
            break;
            case 5:
            if(!passiveLevel[4,4]){
                passiveLevel[4,4] = true;
                magScale += 0.9f;
                mag.transform.localScale = new Vector3(magScale,magScale,0);
            }
            break;
        }
        //크리티컬 확률 및 데미지
        switch(weaponLevel[11]){
            case 1:
                if(!passiveLevel[5,0]){
                    passiveLevel[5,0] = true;
                    criticalDamage += 0.05f;
                    criticalChance += 4;
                }
            break;
            case 2:
                if(!passiveLevel[5,1]){
                    passiveLevel[5,1] = true;
                    criticalDamage += 0.05f;
                    criticalChance += 5;
                }
            break;
            case 3:
                if(!passiveLevel[5,2]){
                    passiveLevel[5,2] = true;
                    criticalDamage += 0.05f;
                    criticalChance += 5;
                }
            break;
            case 4:
                if(!passiveLevel[5,3]){
                    passiveLevel[5,3] = true;
                    criticalDamage += 0.05f;
                    criticalChance += 5;
                }
            break;
            case 5:
                if(!passiveLevel[5,4]){
                    passiveLevel[5,4] = true;
                    criticalDamage += 0.05f;
                    criticalChance += 5;
                }
            break;
        }
    }
    void Reload()
    {
        passiveHealingTimeCount += Time.deltaTime;
        realDelay = maxShootDelay * passiveShootDelay;
        curShootDelay += Time.deltaTime;
        weapon4Count += Time.deltaTime;
        weapon4MaxCount = maxShootDelay/weapon4Delay;
    }
    void WeaponShovel(float dmg,int num)
    {
        for (int i=0;i<num;i++){
            float ranX = Random.Range(-2f,2f);
            float ranZ = Random.Range(-2f,2f);
            GameObject bullet = objectManager.MakeObj("BulletPlayer0");
            bullet.transform.position = transform.position;
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;

            //회전 함수
            Vector3 dirVec = new Vector3(ranX, -7.5f, ranZ);
            rigid.AddForce(dirVec, ForceMode2D.Impulse);
        }
    }
    void WeaponTrident(float dmg, float scale, bool maxLevel)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer1");
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bullet.transform.position = transform.position;
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        bulletLogic.power = dmg * power;

        //회전 함수
        float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
        bullet.transform.rotation = Quaternion.Euler(0,0,degree + 90f);
        bullet.transform.localScale = new Vector3(scale,scale,0);
        Vector3 dirVec = targetImage.transform.position - transform.position;
        rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);
        if(maxLevel){
            //뒤
            GameObject bullet2 = objectManager.MakeObj("BulletPlayer1");
            Bullet bulletLogic2 = bullet2.GetComponent<Bullet>();
            bullet2.transform.position = transform.position;
            Rigidbody2D rigid2 = bullet2.GetComponent<Rigidbody2D>();
            bulletLogic2.power = dmg * power;

            //회전 함수
            float degree2 = Mathf.Atan2(transform.position.y+targetImage.transform.position.y,transform.position.x+targetImage.transform.position.x)*180f/Mathf.PI;
            bullet2.transform.rotation = Quaternion.Euler(0,0,degree2 + 90f);
            bullet2.transform.localScale = new Vector3(scale,scale,0);
            Vector3 dirVec2 = targetImage.transform.position - transform.position;
            rigid2.AddForce(-dirVec2.normalized * bulletSpeed, ForceMode2D.Impulse);
        }
    }
    void WeaponIceSpear(float dmg, float scale, bool maxLevel)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer3");
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bullet.transform.position = transform.position;
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        bulletLogic.power = dmg * power;
        bulletLogic.maxLevel = maxLevel;
        
        //회전 함수
        float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
        bullet.transform.rotation = Quaternion.Euler(0,0,degree + 90f);
                
        bullet.transform.localScale = new Vector3(scale,scale,0);
        Vector3 dirVec = targetImage.transform.position - transform.position;
        rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);
    }
    void WeaponMachineGun(float dmg)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer4");
        Vector3 ranVec = new Vector3(Random.Range(-0.2f,0.3f),Random.Range(-0.2f,0.3f),0);
        bullet.transform.position = transform.position + ranVec;
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.power = dmg * power;

        //회전 함수
        float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
        bullet.transform.rotation = Quaternion.Euler(0,0,degree + 90f);
                
        Vector3 dirVec = (targetImage.transform.position - transform.position)*Random.Range(0.00f,0.10f);
        rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);
        weapon4Count = 0;
    }
    void WeaponShotgun(float dmg, int count, bool maxLevel)
    {
        if(!maxLevel){
            for(int index = 0;index < count;index++){
                GameObject bullet = objectManager.MakeObj("BulletPlayer5");
                bullet.transform.position = transform.position;
                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                Bullet bulletLogic = bullet.GetComponent<Bullet>();
                bulletLogic.power = dmg * power;
                Vector3 ranVec = new Vector3(Random.Range(-0.2f,0.3f)*count,Random.Range(-0.2f,0.3f)*count,0);

                //회전 함수
                float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
                bullet.transform.rotation = Quaternion.Euler(0,0,degree + 90f);

                Vector3 dirVec = targetImage.transform.position - transform.position + ranVec;
                rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);
            }
        } else {
            for(int index = 0;index < count;index++){
                GameObject bullet = objectManager.MakeObj("BulletPlayer5");
                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                Bullet bulletLogic = bullet.GetComponent<Bullet>();
                bulletLogic.power = dmg * power;
                bullet.transform.position = transform.position;
                bullet.transform.rotation = Quaternion.identity;
                Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * index / count), Mathf. Sin(Mathf.PI * 2 * index / count));
                rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);

                //총알 회전 로직
                Vector3 rotVec = Vector3.forward * 360 * index / count + Vector3.forward * 90;
                bullet.transform.Rotate(rotVec);
            }
        }
    }
    void WeaponWindForce(float dmg, float scale, float timer)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer6");
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.waepon6Timer = timer;

        bulletLogic.power = dmg * power;
        bullet.transform.localScale = new Vector3(scale,scale,0);
        bullet.transform.position = transform.position;
        weapon6Count = 0;
    }
    void WeaponFireBall(float dmg, float scale, bool maxLevel)
    {
        if(!maxLevel){
            GameObject bullet = objectManager.MakeObj("BulletPlayer7");
            bullet.transform.position = transform.position;
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();

            bulletLogic.power = dmg * power;
            bullet.transform.localScale = new Vector3(scale,scale,0);
            Vector3 dirVec = targetImage.transform.position - transform.position;
            rigid.AddForce(dirVec.normalized * bulletSpeed/3, ForceMode2D.Impulse);
        } else {
            if(weaponCrazyFireCount>=3){
                weaponCrazyFireCount=0;
            }else if(weaponCrazyFireCount==2){
                for (int i = 0;i<10;i++ ){
                    GameObject bullet = objectManager.MakeObj("BulletPlayer7");
                    bullet.transform.position = transform.position;
                    Bullet bulletLogic = bullet.GetComponent<Bullet>();
                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                    bulletLogic.matchCount += 2;

                    bulletLogic.power = dmg;
                    bullet.transform.localScale = new Vector3(scale,scale,0);
                
                    //회전 함수
                    Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * i / 10), Mathf. Sin(Mathf.PI * 2 * i / 10));
                    rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);

                    //총알 회전 로직
                    Vector3 rotVec = Vector3.forward * 360 * i / 10 + Vector3.forward * 90;
                    bullet.transform.Rotate(rotVec);
                }
                weaponCrazyFireCount++;
            } else if(weaponCrazyFireCount==1) {
                for (int i = 0;i<6;i++ ){
                    GameObject bullet = objectManager.MakeObj("BulletPlayer7");
                    bullet.transform.position = transform.position;
                    Bullet bulletLogic = bullet.GetComponent<Bullet>();
                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                    bulletLogic.matchCount += 2;

                    bulletLogic.power = dmg;
                    bullet.transform.localScale = new Vector3(scale,scale,0);

                    //회전 함수
                    Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * i / 6), Mathf. Sin(Mathf.PI * 2 * i / 6));
                    rigid.AddForce(-dirVec.normalized * bulletSpeed/2, ForceMode2D.Impulse);

                    //총알 회전 로직
                    Vector3 rotVec = Vector3.forward * 360 * i / 6 + Vector3.forward * 90;
                    bullet.transform.Rotate(rotVec);
                }
                weaponCrazyFireCount++;
            } else {
                for (int i = 0;i<3;i++ ){
                    GameObject bullet = objectManager.MakeObj("BulletPlayer7");
                    bullet.transform.position = transform.position;
                    Bullet bulletLogic = bullet.GetComponent<Bullet>();
                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                    bulletLogic.matchCount += 2;

                    bulletLogic.power = dmg;
                    bullet.transform.localScale = new Vector3(scale,scale,0);

                    //회전 함수
                    Vector2 dirVec = new Vector2(Mathf.Cos(Mathf.PI * 2 * i /3), Mathf. Sin(Mathf.PI * 2 * i /3));
                    rigid.AddForce(-dirVec.normalized * bulletSpeed/3, ForceMode2D.Impulse);

                    //총알 회전 로직
                    Vector3 rotVec = Vector3.forward * 360 * i /3 + Vector3.forward * 90;
                    bullet.transform.Rotate(rotVec);
                }
                weaponCrazyFireCount++;
            }
        }
    }
    void WeaponChainLightning(float dmg, int num, bool maxLevel)
    {
        if(!maxLevel){
            for(int index = 0;index < num;index++){
                float ranX = Random.Range(-2.0f,2.0f);
                float ranY = Random.Range(-1.0f,7.0f);
                GameObject bullet8 = objectManager.MakeObj("BulletPlayer8");
                bullet8.transform.position = new Vector3(transform.position.x+ranX, transform.position.y+ranY);
                Rigidbody2D rigid8 = bullet8.GetComponent<Rigidbody2D>();
                Bullet bulletLogic8 = bullet8.GetComponent<Bullet>();
                bulletLogic8.power = dmg * power;
            }
        } else {
            for(int index = 0;index < num;index++){
                float ranX = Random.Range(-2.0f,2.0f);
                float ranY = Random.Range(-1.0f,7.0f);
                float ranScaleX = Random.Range(1f,2f);
                float ranScaleY = Random.Range(1.5f,2.5f);
                GameObject bullet8 = objectManager.MakeObj("BulletPlayer8");
                bullet8.transform.localScale = new Vector3(ranScaleX,ranScaleY,0);
                bullet8.transform.position = new Vector3(transform.position.x+ranX, transform.position.y+ranY);
                Rigidbody2D rigid8 = bullet8.GetComponent<Rigidbody2D>();
                Bullet bulletLogic8 = bullet8.GetComponent<Bullet>();
                bulletLogic8.power = dmg * power;
            }
        }
    }
    void WeaponPickaxe(float dmg, int num, bool maxLevel)
    {
        for(int index = 0;index < num;index++){
            float ranX = Random.Range(-2f,2f);
            float ranZ = Random.Range(-2f,2f);
            GameObject bullet = objectManager.MakeObj("BulletPlayer10");
            bullet.transform.position = transform.position;
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;
            if(maxLevel){
                bullet.transform.localScale = new Vector3(2,2,0);
            }
            //회전 함수
            Vector3 dirVec = new Vector3(ranX, 7.5f, ranZ);
            rigid.AddForce(dirVec, ForceMode2D.Impulse);
        }
    }
    void WeaponPistol(float dmg, float scale)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer11");
        bullet.transform.position = transform.position;
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.power = dmg * power;
        bullet.transform.localScale = new Vector3(scale,scale,0);

        //회전 함수
        float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
        bullet.transform.rotation = Quaternion.Euler(0,0,degree + 90f);
                
        Vector3 dirVec = targetImage.transform.position - transform.position;
        rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);
    }
    // 쓰나미
    void WeaponTsunami(float dmg)
    {
        for(int i =0;i<9;i++){
            GameObject bullet = objectManager.MakeObj("BulletPlayer13");
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;
            Vector3 vec =  new Vector3(transform.position.x - 3.5f,transform.position.y+(4f-i),0);
            bullet.transform.position = vec;
            rigid.AddForce(Vector2.right * bulletSpeed, ForceMode2D.Impulse);
        }
        tsunamiCount=0;
    }
    // 메테오
    void WeaponMeteo(float dmg)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer14");
        bullet.transform.position = new Vector3(transform.position.x -4,transform.position.y+7,0);
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.power = dmg;
        rigid.AddForce(Vector2.right*2, ForceMode2D.Impulse);
        rigid.AddForce(Vector2.down*0.5f, ForceMode2D.Impulse);
    }
    // 돌아가는 망치
    void ThrowHammer(float dmg, bool maxlevel)
    {
        if(!maxlevel){
            GameObject bullet = objectManager.MakeObj("BulletPlayer16");
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;
            bulletLogic.maxLevel = maxlevel;
            bulletLogic.playerCurVec = transform.position;
        } else {
            GameObject bullet = objectManager.MakeObj("BulletPlayer16");
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;
            bulletLogic.playerCurVec = transform.position;

            GameObject bullet1 = objectManager.MakeObj("BulletPlayer16");
            Rigidbody2D rigid1 = bullet1.GetComponent<Rigidbody2D>();
            Bullet bulletLogic1 = bullet1.GetComponent<Bullet>();
            bulletLogic1.power = dmg * power;
            bulletLogic1.maxLevel = maxlevel;
            bulletLogic1.playerCurVec = transform.position;
        }
    }
    //철퇴 공격
    void MaceAttack(float dmg, float maceSpeed, bool maxlevel)
    {
        if(maxlevel){
            GameObject bullet = objectManager.MakeObj("BulletPlayer17");
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;
            bullet.transform.localScale = new Vector3(0.5f,0.5f,0);
            
            Vector3 dirVec = targetImage.transform.position - transform.position;
            bullet.transform.position = transform.position + dirVec.normalized;
            bulletLogic.bulletSpeed = maceSpeed;
            //회전 함수
            float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
            bullet.transform.rotation = Quaternion.Euler(0,0,degree + 90f);
            bulletLogic.playerCurVec = transform.position;
        } else {
            GameObject bullet = objectManager.MakeObj("BulletPlayer17");
            Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;
            
            Vector3 dirVec = targetImage.transform.position - transform.position;
            bullet.transform.position = transform.position + dirVec.normalized;
            bulletLogic.bulletSpeed = maceSpeed;
            //회전 함수
            float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
            bullet.transform.rotation = Quaternion.Euler(0,0,degree + 90f);
            bulletLogic.playerCurVec = transform.position;
        }
    }
    //말타고 달리기
    void HorseAttack(float dmg, bool maxLevel)
    {
        horseAttackObj.SetActive(true);
        Bullet bulletLogic = horseAttackObj.GetComponent<Bullet>();

        bulletLogic.power = dmg * power;
        if(maxLevel){
            PointEffector2D point = horseAttackObj.GetComponent<PointEffector2D>();
            point.forceMagnitude = 400;
        }
    }
    // 십자가 공격
    void CrossAttack(float dmg, bool maxLevel)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer19");
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.power = dmg * power;
        bullet.transform.position = transform.position;
        if(maxLevel){
            bulletLogic.maxLevel = maxLevel;
            bulletLogic.playerCurVec = transform.position;
        }
    }
    // 빔
    void LightBeam(float dmg, float speed)
    {
        Bullet bulletLogic = lightBeam.GetComponent<Bullet>();
        bulletLogic.power = dmg * power;
        bulletLogic.bulletSpeed = speed;
    }
    // 에너지 파동
    void EnergyForce(float dmg, bool maxLevel)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer21");
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        bulletLogic.power = dmg * power;
        Vector3 dirVec = targetImage.transform.position - transform.position;
        bullet.transform.position = transform.position;

        //회전 함수
        float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
        bullet.transform.rotation = Quaternion.Euler(0,0,degree + 180f);
        rigid.AddForce(dirVec.normalized * 0.2f, ForceMode2D.Impulse);
        bulletLogic.playerCurVec = dirVec.normalized;
        bulletLogic.maxLevel = maxLevel;
    }
    // 에너지 파동2
    void EnergyForce2(float dmg, bool maxLevel)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer22");
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        bulletLogic.power = dmg * power;
        Vector3 dirVec = targetImage.transform.position - transform.position;
        bullet.transform.position = transform.position;

        //회전 함수
        float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
        bullet.transform.rotation = Quaternion.Euler(0,0,degree + 180f);
        rigid.AddForce(dirVec.normalized * bulletSpeed, ForceMode2D.Impulse);
        bulletLogic.maxLevel = maxLevel;
    }
    // 랜덤하게 스킬사용
    void RandomThrow(int level, bool maxLevel)
    {
        float ran = Random.Range(0.0f,100.0f);
        if(ran<12.5f){ // 12.5% 곡괭이
            WeaponPickaxe(120 * level,1 * level,maxLevel);
        } else if (ran<25f){ // 12.5% 아이스 스피어
            WeaponIceSpear(50 * level, 1f,maxLevel);
        } else if (ran<37.5f){ // 12.5% 삼지창
            WeaponTrident(80 * level,0.5f + level/10,maxLevel);
        } else if(ran<50f){ // 12.5% 돌아가는 망치
            ThrowHammer(50 * level,maxLevel);
        } else if(ran<62.5f){ // 12.5% 윈드 포스
            if(maxLevel){
                WeaponWindForce(30 * level,1.5f,1f);
            } else {
                WeaponWindForce(30 * level,1f,1f);
            }
        } else if(ran<75f){ // 12.5% 삽
            if(maxLevel){
                WeaponShovel(100 * level,2 * level);
            } else{
                WeaponShovel(100 * level,1 * level);
            }
        } else if(ran<87.5f){ // 12.5% 체인 라이트닝
            WeaponChainLightning(40 * level, 1+level,maxLevel);
        } else if(ran<100.0f){ // 12.5% 십자가 공격
            CrossAttack(40 * level,maxLevel);
        }
    }
    // 지뢰
    void TrapCreat(float dmg, float scale)
    {
        float ranX = Random.Range(-1.5f,1.5f);
        float ranY = Random.Range(-4.5f,4.5f);
        GameObject bullet = objectManager.MakeObj("BulletPlayer23");
        bullet.transform.position = new Vector3(transform.position.x+ranX, transform.position.y+ranY);
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.power = dmg * power;
        bullet.transform.localScale = new Vector3(scale,scale,0);
    }
    // 터렛
    void TurretCreat(float dmg, float scale, bool maxLevel)
    {
        if(maxLevel){
            if(turretObj.activeSelf){
                PlayerObj turret = turretObj.GetComponent<PlayerObj>();
                turret.power = dmg * power;
                turret.Fire(maxLevel);
            } else {
                turretObj.transform.localScale = new Vector3(scale, scale, 0);
                turretObj.SetActive(true);
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                turretObj.transform.position = pos;
                PlayerObj turret = turretObj.GetComponent<PlayerObj>();
                turret.power = dmg * power;
                turret.maxLevel = maxLevel;
            }
        } else {
            if(turretObj.activeSelf){
                PlayerObj turret = turretObj.GetComponent<PlayerObj>();
                turret.power = dmg * power;
                turret.Fire(maxLevel);
            } else {
                PlayerObj turret = turretObj.GetComponent<PlayerObj>();
                turretObj.transform.localScale = new Vector3(scale, scale, 0);
                turretObj.SetActive(true);
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                turretObj.transform.position = pos;
            }
        }
    }
    // 활 던지기
    void BowThrow(float dmg, int count, bool maxLevel)
    {
        GameObject bullet = objectManager.MakeObj("BulletPlayer25");
        bullet.transform.position = transform.position;
        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        Bullet bulletLogic = bullet.GetComponent<Bullet>();
        bulletLogic.power = dmg * power;

        Vector3 dirVec = targetImage.transform.position - transform.position;
        //회전 함수
        float degree = Mathf.Atan2(transform.position.y-targetImage.transform.position.y,transform.position.x-targetImage.transform.position.x)*180f/Mathf.PI;
        bullet.transform.rotation = Quaternion.Euler(0,0,degree + 180f);
        rigid.AddForce(dirVec.normalized * bulletSpeed * 1.5f, ForceMode2D.Impulse);
        bulletLogic.matchCount += count;
        bulletLogic.maxLevel = maxLevel;
    }
    // 곰
    void Bear(float dmg, float objLife, bool maxLevel)
    {
        if(bearObj.activeSelf){
            PlayerObj bear = bearObj.GetComponent<PlayerObj>();
            bear.power = dmg * power;
            bear.Fire(maxLevel);
        } else {
            PlayerObj bear = bearObj.GetComponent<PlayerObj>();
            bear.life = objLife;
            bearObj.SetActive(true);
            bear.power = dmg * power;
            Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
            bearObj.transform.position = pos;
        }
        if(maxLevel){
            PlayerObj bear = bearObj.GetComponent<PlayerObj>();
            bear.power = dmg * power;
            bear.speed = 2f;
            bear.maxLevel = maxLevel;
        } else {
            PlayerObj bear = bearObj.GetComponent<PlayerObj>();
            bear.speed = 1f;
        }
    }
    // 스켈레톤
    void SkullCreat(float dmg, float objLife, int count, bool maxLevel)
    {
        if(skullCount < count){
            int ran = Random.Range(0,2);
            if(ran == 0){ // 스켈레톤
                GameObject skull = objectManager.MakeObj("BulletPlayer28");
                PlayerObj skullLogic = skull.GetComponent<PlayerObj>();
                skullLogic.life = objLife;
                skullLogic.type = "Skull";
                skullLogic.power = dmg * power;
                skullLogic.maxLevel = maxLevel;
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                skull.transform.position = pos;
                skullCount++;
            } else if(ran == 1){ // 궁수
                GameObject bowSkull = objectManager.MakeObj("BulletPlayer28");
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                PlayerObj bowSkullLogic = bowSkull.GetComponent<PlayerObj>();
                bowSkullLogic.life = objLife*0.8f;
                bowSkullLogic.type = "BowSkull";
                bowSkullLogic.power = (dmg*1.1f) * power;
                bowSkullLogic.maxLevel = maxLevel;
                bowSkull.transform.position = pos;
                skullCount++;
            }
        }
    }
    // 새
    void BirdCreat(float dmg, int count, bool maxLevel)
    {
        int ran = Random.Range(0,2);
        PlayerObj birdLogic = birdObj.GetComponent<PlayerObj>();
        birdLogic.power = dmg;
        if(!birdObj.activeSelf){
            birdObj.SetActive(true);    
        } else {
            if(maxLevel){
                // 가로
                GameObject bullet = objectManager.MakeObj("BulletPlayer32");
                Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                Bullet bulletLogic = bullet.GetComponent<Bullet>();
                bulletLogic.power = dmg * power;
                bulletLogic.matchCount += count;

                bullet.transform.position = new Vector2(targetImage.transform.position.x, transform.position.y+5f);
                //회전 함수
                bullet.transform.rotation = Quaternion.Euler(0,0,180f);
                rigid.AddForce(Vector2.down * bulletSpeed, ForceMode2D.Impulse);

                // 세로
                GameObject bullet1 = objectManager.MakeObj("BulletPlayer32");
                Rigidbody2D rigid1 = bullet1.GetComponent<Rigidbody2D>();
                Bullet bulletLogic1 = bullet1.GetComponent<Bullet>();
                bulletLogic1.power = dmg * power;
                bulletLogic1.matchCount += count;

                bullet1.transform.position = new Vector2(transform.position.x+4f, targetImage.transform.position.y);
                //회전 함수
                bullet1.transform.rotation = Quaternion.Euler(0,0,90f);
                rigid1.AddForce(Vector2.left * bulletSpeed, ForceMode2D.Impulse);
            } else {
                if(ran ==0){ // 세로 공격
                    GameObject bullet = objectManager.MakeObj("BulletPlayer32");
                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                    Bullet bulletLogic = bullet.GetComponent<Bullet>();
                    bulletLogic.power = dmg * power;
                    bulletLogic.matchCount += count;

                    bullet.transform.position = new Vector2(targetImage.transform.position.x, transform.position.y+5f);
                    //회전 함수
                    bullet.transform.rotation = Quaternion.Euler(0,0,180f);
                    rigid.AddForce(Vector2.down * bulletSpeed, ForceMode2D.Impulse);
                } else { //가로 공격
                    GameObject bullet = objectManager.MakeObj("BulletPlayer32");
                    Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
                    Bullet bulletLogic = bullet.GetComponent<Bullet>();
                    bulletLogic.power = dmg * power;
                    bulletLogic.matchCount += count;

                    bullet.transform.position = new Vector2(transform.position.x+4f, targetImage.transform.position.y);
                    //회전 함수
                    bullet.transform.rotation = Quaternion.Euler(0,0,90f);
                    rigid.AddForce(Vector2.left * bulletSpeed, ForceMode2D.Impulse);
                }
            }
        }
    }
    // 뱀
    void SnakeCreat(float dmg, float scale, bool maxLevel)
    {
        if(!snake){
            snake = true;
            GameObject bullet = objectManager.MakeObj("BulletPlayer33");
            bullet.transform.localScale = new Vector3(scale, scale, 0);
            bullet.transform.position = transform.position;
            Bullet bulletLogic = bullet.GetComponent<Bullet>();
            bulletLogic.power = dmg * power;
            if(maxLevel){
                PointEffector2D bulletPointer = bullet.GetComponent<PointEffector2D>();
                bulletPointer.forceMagnitude = -30;
            }
        }
    }
    // 골렘
    void Golem(float dmg, float life, bool maxLevel)
    {
        int ran = Random.Range(0,6);
        if(ran==0){ // 아이스 골렘
            if(!golemObj.activeSelf){
                isGolem = true;
                golemObj.SetActive(true);
                PlayerObj golemLogic = golemObj.GetComponent<PlayerObj>();
                golemLogic.type = "IceGolem";
                golemLogic.life = life;
                golemLogic.power = dmg * power;
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                golemObj.transform.position = pos;
                golemLogic.maxLevel = maxLevel;
            } else {
                PlayerObj golem = golemObj.GetComponent<PlayerObj>();
                golem.power = dmg * power;
                golem.Fire(maxLevel);
            }
        } else if(ran==1){ // 파이어 골렘
            if(!golemObj.activeSelf){
                isGolem = true;
                golemObj.SetActive(true);
                PlayerObj golemLogic = golemObj.GetComponent<PlayerObj>();
                golemLogic.type = "FireGolem";
                golemLogic.life = life;
                golemLogic.power = (dmg * power) * 0.8f;
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                golemObj.transform.position = pos;
                golemLogic.maxLevel = maxLevel;
            } else {
                PlayerObj golem = golemObj.GetComponent<PlayerObj>();
                golem.power = dmg * power;
                golem.Fire(maxLevel);
            }
        } else if(ran==3){ // 돌 골렘
            if(!golemObj.activeSelf){
                isGolem = true;
                golemObj.SetActive(true);
                PlayerObj golemLogic = golemObj.GetComponent<PlayerObj>();
                golemLogic.type = "StoneGolem";
                golemLogic.life = life;
                golemLogic.power = dmg * power;
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                golemObj.transform.position = pos;
                golemLogic.maxLevel = maxLevel;
            } else {
                PlayerObj golem = golemObj.GetComponent<PlayerObj>();
                golem.power = dmg * power;
                golem.Fire(maxLevel);
            }
        } else if(ran==4){ // 물 골렘
            if(!golemObj.activeSelf){
                isGolem = true;
                golemObj.SetActive(true);
                PlayerObj golemLogic = golemObj.GetComponent<PlayerObj>();
                golemLogic.type = "WaterGolem";
                golemLogic.life = life;
                golemLogic.power = dmg * power;
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                golemObj.transform.position = pos;
                golemLogic.maxLevel = maxLevel;
             } else {
                PlayerObj golem = golemObj.GetComponent<PlayerObj>();
                golem.power = dmg * power;
                golem.Fire(maxLevel);
            }
        } else { // 전기 골렘
            if(!golemObj.activeSelf){
                isGolem = true;
                golemObj.SetActive(true);
                PlayerObj golemLogic = golemObj.GetComponent<PlayerObj>();
                golemLogic.type = "LightningGolem";
                golemLogic.life = life;
                golemLogic.power = (dmg * power)*0.8f;
                Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
                golemObj.transform.position = pos;
                golemLogic.maxLevel = maxLevel;
            } else {
                PlayerObj golem = golemObj.GetComponent<PlayerObj>();
                golem.power = dmg * power;
                golem.Fire(maxLevel);
            }
        }
    }
    // 호수
    void LakeCreate(float dmg, bool maxLevel)
    {
        if(!isLake){
            isLake=true;
            GameObject bullet = objectManager.MakeObj("BulletPlayer35");
            PlayerObj lakeLogic = bullet.GetComponent<PlayerObj>();
            lakeLogic.power = dmg * power;
            Vector2 pos = new Vector2(transform.position.x+1f,transform.position.y);
            bullet.transform.position = pos;
            lakeLogic.maxLevel = maxLevel;
        }
    }

    // 치명타
    public float CriticalHit(float dmg)
    {
        int ran = Random.Range(1,101);
        if(criticalChance >= ran){
            dmg = criticalDamage * dmg;
            isCritical = true;
        } else {
            isCritical = false;
        }
        return dmg;
    }
    void TimeStopChance()
    {
        gameManager.playerDeadChance();
        Time.timeScale = 0;
        CancelInvoke("TimeStopChance");
    }
    void TimeStopDead()
    {
        gameManager.playerDead();
        Time.timeScale = 0;
        CancelInvoke("TimeStopDead");
    }
    public void Healing(float healValue)
    {
        if(gameManager.character=="Hunter"){
            healValue *= 2f;
        }
        if(life+healValue>maxLife){
            gameManager.DamageText(transform.position, maxLife - life, "Healing", false);
            life = maxLife;
        } else if (life>=maxLife){
            gameManager.DamageText(transform.position, 0, "Healing", false);
            life = maxLife;
        } else {
            gameManager.DamageText(transform.position, healValue, "Healing", false);
            life += healValue;
        }
    }
    public void Boom(float dmg, bool isBoom)
    {
        //화면 에있는 모든 적들에게 에게 데미지를 준다
        boomEffect.SetActive(true);
        GameObject[] enemyA = objectManager.GetPool("EnemyA");
        GameObject[] enemyB = objectManager.GetPool("EnemyB");
        GameObject[] enemyC = objectManager.GetPool("EnemyC");
        GameObject[] enemyD = objectManager.GetPool("EnemyD");
        GameObject[] enemyE = objectManager.GetPool("EnemyE");
        for(int index=0; index<enemyA.Length;index++){
            if(objectManager.GetPool("EnemyA")[index].activeSelf){
                Vector3 screenPoint = cam.WorldToViewportPoint(objectManager.GetPool("EnemyA")[index].transform.position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                if(onScreen){
                    EnemyMove enemyLogicA = enemyA[index].GetComponent<EnemyMove>();
                    enemyLogicA.OnHit(dmg);
                }
            }
        }
        for(int index=0; index<enemyB.Length;index++){
            if(objectManager.GetPool("EnemyB")[index].activeSelf){
                Vector3 screenPoint = cam.WorldToViewportPoint(objectManager.GetPool("EnemyB")[index].transform.position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                if(onScreen){
                    EnemyMove enemyLogicB = enemyB[index].GetComponent<EnemyMove>();
                    enemyLogicB.OnHit(dmg);
                }
            }
        }
        for(int index=0; index<enemyC.Length;index++){
            if(objectManager.GetPool("EnemyC")[index].activeSelf){
                Vector3 screenPoint = cam.WorldToViewportPoint(objectManager.GetPool("EnemyC")[index].transform.position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                if(onScreen){
                    EnemyMove enemyLogicC = enemyC[index].GetComponent<EnemyMove>();
                    enemyLogicC.OnHit(dmg);
                }
            }
        }
        for(int index=0; index<enemyD.Length;index++){
            if(objectManager.GetPool("EnemyD")[index].activeSelf){
                Vector3 screenPoint = cam.WorldToViewportPoint(objectManager.GetPool("EnemyD")[index].transform.position);
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                if(onScreen){
                    EnemyMove enemyLogicD = enemyD[index].GetComponent<EnemyMove>();
                    enemyLogicD.OnHit(dmg);
                }
            }
        }
        // 보스는 사라지기에 그냥 놔둔다;
        if(objectManager.GetPool("EnemyE")[0].activeSelf){
            Vector3 screenPoint = cam.WorldToViewportPoint(objectManager.GetPool("EnemyE")[0].transform.position);
            bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            if(onScreen){
                EnemyMove enemyLogicE = enemyE[0].GetComponent<EnemyMove>();
                enemyLogicE.OnHit(dmg);
            }
        }
        if(!isBoom)
            return;

        // 모든 적군의 총알을 없앤다
        GameObject[] bulletA = objectManager.GetPool("BulletEnemyA");
        GameObject[] bulletB = objectManager.GetPool("BulletEnemyB");
        for(int index=0; index<bulletA.Length;index++){
            bulletA[index].SetActive(false);
            bulletB[index].SetActive(false);
        }
    }
    void Shield()
    {
        if(isShield){
            shieldObj.SetActive(true);
        } else {
            shieldObj.SetActive(false);
        }
    }
    public void EnemyOff()
    {
        //화면의 적군을 지우기 아직 사용x
        boomEffect.SetActive(true);
        GameObject[] enemyA = objectManager.GetPool("EnemyA");
        GameObject[] enemyB = objectManager.GetPool("EnemyB");
        GameObject[] enemyC = objectManager.GetPool("EnemyC");
        for(int index=0; index<enemyA.Length;index++){
            if(objectManager.GetPool("EnemyA")[index].activeSelf){
                objectManager.GetPool("EnemyA")[index].SetActive(false);
            }
        }
        for(int index=0; index<enemyB.Length;index++){
            if(objectManager.GetPool("EnemyB")[index].activeSelf){
                objectManager.GetPool("EnemyB")[index].SetActive(false);
            }
        }
        for(int index=0; index<enemyC.Length;index++){
            if(objectManager.GetPool("EnemyC")[index].activeSelf){
                objectManager.GetPool("EnemyC")[index].SetActive(false);
            }
        }
        // 모든 적군의 총알을 없앤다
        GameObject[] bulletA = objectManager.GetPool("BulletEnemyA");
        GameObject[] bulletB = objectManager.GetPool("BulletEnemyB");
        for(int index=0; index<bulletA.Length;index++){
            bulletA[index].SetActive(false);
            bulletB[index].SetActive(false);
        }
    }
}