using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("Visuals & Skins")]
    public SpriteRenderer sr;
    public Sprite normalSkin, powerSkin;

    [Header("Enemy Slots")]
    public GameObject enemy; 
    public GameObject ghostVariant, redVariant, greenVariant, octopusVariant, deathVariant, orangeVariant;
    public GameObject bossPrefab, fireballPrefab;

    [Header("Food Prefabs")]
    public GameObject foodPrefab, bananaPrefab, pringlePrefab, pearPrefab, powerPrefab;

    [Header("Audio")]
    public AudioSource musicSource; 
    public AudioSource powerMusicSource; 
    public AudioSource sfxSource;   
    public AudioClip hitSound, eatSound, arrivalSound, powerSound, bossVoiceLine, shootSound;

    [Header("UI")]
    public TextMeshProUGUI gameOverText;
    public Slider bossHealthSlider;

    private float moveSpeed = 7f;
    private float defaultSpeed;
    private bool isPoweredUp = false;
    private bool gameFinished = false;
    private bool bossActive = false;
    private bool facingRight = true;
    
    // Player HP (100 / 20 = 5 hits)
    private float playerHP = 100f;

    private List<GameObject> activePool = new List<GameObject>();
    private List<GameObject> queue = new List<GameObject>();

    void Start()
    {
        defaultSpeed = moveSpeed;
        if (bossHealthSlider) bossHealthSlider.gameObject.SetActive(false);
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (enemy) activePool.Add(enemy);
        if (ghostVariant) queue.Add(ghostVariant);
        if (redVariant) queue.Add(redVariant);
        if (greenVariant) queue.Add(greenVariant);
        if (octopusVariant) queue.Add(octopusVariant);
        if (deathVariant) queue.Add(deathVariant);
        if (orangeVariant) queue.Add(orangeVariant);

        if (musicSource) { musicSource.Stop(); musicSource.Play(); }
        if (powerMusicSource) powerMusicSource.Stop();

        InvokeRepeating(nameof(SpawnEnemyLogic), 0.5f, 1.5f);
        InvokeRepeating(nameof(SpawnFood), 1f, 1.2f);
    }

    void Update()
    {
        if (gameFinished)
        {
            if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        MovePlayer();

        if (isPoweredUp || bossActive) {
            ApplyMagnetEffect();
            if (Input.GetKeyDown(KeyCode.Space)) ShootBlueFireball();
        }
    }

    void MovePlayer()
    {
        float move = Input.GetAxisRaw("Horizontal");
        transform.Translate(Vector2.right * move * moveSpeed * Time.deltaTime);
        
        if (move > 0) { sr.flipX = false; facingRight = true; }
        else if (move < 0) { sr.flipX = true; facingRight = false; }

        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect; 
        Vector3 pos = transform.position;
        if (pos.x > screenWidth + 0.5f) pos.x = -screenWidth - 0.4f;
        else if (pos.x < -screenWidth - 0.5f) pos.x = screenWidth + 0.4f;
        transform.position = pos;
    }

    void ShootBlueFireball()
    {
        GameObject bolt = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        bolt.tag = "PlayerProjectile"; 
        SpriteRenderer boltSr = bolt.GetComponent<SpriteRenderer>();
        if (boltSr != null) {
            boltSr.color = Color.cyan;
            boltSr.flipX = !facingRight; 
        }
        var mover = bolt.AddComponent<InternalMover>();
        mover.direction = facingRight ? Vector2.right : Vector2.left; 
        mover.speed = 12f;
        if (sfxSource && shootSound) sfxSource.PlayOneShot(shootSound);
    }

    void ActivatePower()
    {
        if (isPoweredUp) CancelInvoke(nameof(DeactivatePower));
        isPoweredUp = true;
        moveSpeed = defaultSpeed * 1.5f;
        sr.sprite = powerSkin;
        if (musicSource) musicSource.Stop();
        if (powerMusicSource) { powerMusicSource.Stop(); powerMusicSource.Play(); }
        if (sfxSource && powerSound) sfxSource.PlayOneShot(powerSound);
        Invoke(nameof(DeactivatePower), 20f); 
    }

    void DeactivatePower()
    {
        if (bossActive) return;
        isPoweredUp = false;
        moveSpeed = defaultSpeed;
        sr.sprite = normalSkin;
        if (powerMusicSource) powerMusicSource.Stop();
        if (musicSource) musicSource.Play();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gameFinished) return;

        if (other.CompareTag("Power") || other.name.ToLower().Contains("power"))
        {
            ActivatePower();
            if (sfxSource && eatSound) sfxSource.PlayOneShot(eatSound);
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Food"))
        {
            string n = other.name.ToLower();
            int points = n.Contains("banana") ? 5 : (n.Contains("pear") ? 4 : (n.Contains("pringle") ? 3 : 2));
            ScoreManager.instance.AddPoints(points);
            if (sfxSource && eatSound) sfxSource.PlayOneShot(eatSound);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Enemy"))
        {
            if (!isPoweredUp && !bossActive) 
            {
                Die();
            }
            else 
            {
                playerHP -= 20; 
                if (sfxSource && hitSound) sfxSource.PlayOneShot(hitSound);
                
                // FIX: If it is the boss, don't destroy it. Only destroy small enemies/projectiles.
                if (other.GetComponent<InternalBossManager>() == null) 
                {
                    Destroy(other.gameObject);
                }
                
                if (playerHP <= 0) Die();
            }
        }
    }

    public void StartBossBattle() {
        if (bossActive) return;
        bossActive = true;
        CancelInvoke(nameof(SpawnEnemyLogic)); 

        sr.sprite = powerSkin; 
        moveSpeed = defaultSpeed * 1.5f;

        if (sfxSource && arrivalSound) sfxSource.PlayOneShot(arrivalSound);
        
        GameObject b = Instantiate(bossPrefab, new Vector3(0, 4, 0), Quaternion.identity);
        b.name = "FinalBoss"; 
        b.tag = "Enemy"; 
        b.AddComponent<InternalBossManager>().Setup(fireballPrefab, bossHealthSlider, gameOverText);
        if (bossHealthSlider) bossHealthSlider.gameObject.SetActive(true);
    }

    void SpawnFood() {
        if (gameFinished) return;
        int r = Random.Range(0, 100);
        GameObject f = (r < 10 && !isPoweredUp) ? powerPrefab : (r < 25 ? bananaPrefab : (r < 45 ? pearPrefab : (r < 65 ? pringlePrefab : foodPrefab)));
        SpawnObject(f, f == powerPrefab ? "Power" : "Food", 2.5f, Vector2.down);
    }

    void SpawnEnemyLogic() { 
        if (!bossActive && activePool.Count > 0) 
            SpawnObject(activePool[Random.Range(0, activePool.Count)], "Enemy", 4f, Vector2.down); 
    }

    void SpawnObject(GameObject prefab, string tag, float speed, Vector2 dir) {
        if (!prefab) return;
        float sw = Camera.main.orthographicSize * Camera.main.aspect;
        GameObject obj = Instantiate(prefab, new Vector3(Random.Range(-sw, sw), 6, 0), Quaternion.identity);
        obj.tag = tag;
        var m = obj.AddComponent<InternalMover>();
        m.speed = speed; m.direction = dir;
    }

    void ApplyMagnetEffect() {
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        foreach (GameObject f in foods)
            f.transform.position = Vector2.MoveTowards(f.transform.position, transform.position, 12f * Time.deltaTime);
    }

    public void UnlockNextEnemy() { if (queue.Count > 0) { activePool.Add(queue[0]); queue.RemoveAt(0); } }

    public void Die() {
        gameFinished = true;
        if (musicSource) musicSource.Stop();
        if (powerMusicSource) powerMusicSource.Stop();
        if (gameOverText) { gameOverText.text = "GAME OVER! Press R"; gameOverText.gameObject.SetActive(true); }
    }
}

public class InternalMover : MonoBehaviour {
    public float speed; public Vector2 direction = Vector2.down;
    void Update() { 
        transform.Translate(direction * speed * Time.deltaTime); 
        // Safety: Prevent mover from cleaning up the boss
        if(gameObject.name != "FinalBoss") {
            if(Mathf.Abs(transform.position.y) > 15 || Mathf.Abs(transform.position.x) > 25) Destroy(gameObject); 
        }
    }
}

public class InternalBossManager : MonoBehaviour {
    private GameObject fireball; private Slider hpSlider; private TextMeshProUGUI winUI;
    private float hp = 100f; private float fireTimer = 2f; private bool movingRight = true;

    public void Setup(GameObject fb, Slider slider, TextMeshProUGUI text) { 
        fireball = fb; hpSlider = slider; winUI = text;
        if(hpSlider) { hpSlider.maxValue = 100; hpSlider.value = 100; }
    }

    void Update() {
        if (transform.position.y > 0.5f) transform.Translate(Vector2.down * 2f * Time.deltaTime);
        
        float limit = Camera.main.orthographicSize * Camera.main.aspect - 1.5f;
        transform.Translate((movingRight ? Vector2.right : Vector2.left) * 3f * Time.deltaTime);
        if (transform.position.x > limit) movingRight = false; else if (transform.position.x < -limit) movingRight = true;
        
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0) {
            GameObject f = Instantiate(fireball, transform.position - new Vector3(0, 1.5f, 0), Quaternion.identity);
            f.tag = "Enemy"; 
            SpriteRenderer fSr = f.GetComponent<SpriteRenderer>();
            if (fSr != null) { fSr.color = Color.red; fSr.flipX = movingRight; }
            var mover = f.AddComponent<InternalMover>();
            mover.speed = 7f; 
            mover.direction = movingRight ? Vector2.right : Vector2.left; 
            fireTimer = 1.5f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // BOSS DIES ONLY AFTER 5 HITS (20 hp per blue fireblast)
        if (other.CompareTag("PlayerProjectile")) {
            hp -= 20f; 
            if (hpSlider) hpSlider.value = hp; 
            Destroy(other.gameObject);
            
            if (hp <= 0) {
                if (winUI) { winUI.text = "YOU WIN! Press R"; winUI.gameObject.SetActive(true); }
                Destroy(gameObject);
            }
        }
    }
}