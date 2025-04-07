using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Private Variables")]  // T�tulo para agrupar las variables privadas en el inspector de Unity.
    [Space]

    [Tooltip("La vida m�xima que el jugador puede tener (se puede modificar desde Unity).")]
    [SerializeField] private float maxHealth = 100;
    [Tooltip("Referencia al Rigidbody2D del jugador, usado para aplicar f�sica y movimiento.")]
    [SerializeField] private Rigidbody2D rb;
    [Tooltip("Un transform que indica la posici�n donde se verifica si el jugador est� tocando el suelo.")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("El radio de la zona de verificaci�n del suelo (es un c�rculo).")]
    [SerializeField] private float groundRadius;
    [Tooltip("La capa en la que se consideran los objetos \"suelo\".")]
    [SerializeField] private LayerMask GroundLayer;
    [Tooltip("La capa en la que se consideran los objetos \"barro\", que afectan al movimiento del jugador.")]
    [SerializeField] private LayerMask mudLayer;
    [Tooltip("El sistema de part�culas que se reproducir� cuando el jugador reciba da�o.")]
    [SerializeField] private ParticleSystem hit_ps;
    [Tooltip("El componente Animator para controlar las animaciones del jugador.")]
    [SerializeField] public Animator playerAnimator;

    private float movement;

    [Header("Public Variables")]  // T�tulo para agrupar las variables p�blicas que pueden ser modificadas desde otros scripts o el inspector.
    [Space]

    [Tooltip("La vida actual del jugador.")]
    public float health;
    [Tooltip("La velocidad de movimiento del jugador.")]
    public float speed;
    [Tooltip("La fuerza de salto del jugador.")]
    public float jumpForce;
    [Tooltip("Determina si el jugador puede saltar (est� en el suelo o no en barro).")]
    public bool canJump;
    [Tooltip("El tiempo que el jugador estar� siendo empujado despu�s de recibir un golpe.")]
    public float hitTime;
    [Tooltip("La fuerza de empuje en el eje horizontal cuando el jugador recibe un golpe.")]
    public float hitForceX;
    [Tooltip("La fuerza de empuje en el eje vertical cuando el jugador recibe un golpe.")]
    public float hitForceY;
    [Tooltip("Determina si el golpe fue desde la derecha o no (usado para direccionar el empuje).")]
    public bool hitFromRight;
    [Tooltip("Texto en la UI que muestra la vida del jugador.")]
    public TextMeshProUGUI healthText;
    [Tooltip("La cantidad de part�culas que se generar�n cuando el jugador reciba da�o.")]
    public float particleCount;

    
    [SerializeField] private Sprite[] particleSprites;
    public int spriteIndex;

    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Obtiene el componente Rigidbody2D del jugador.
        playerAnimator = GetComponent<Animator>();  // Obtiene el componente Animator para controlar animaciones.
        hit_ps = GetComponentInChildren<ParticleSystem>();  // Obtiene el sistema de part�culas del jugador (hijo del objeto).
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics2D.OverlapCircle(groundCheck.position, groundRadius, GroundLayer))
        {
            canJump = true;
        }
        else
        {
            canJump = false;
        }

        //canJump = Physics2D.OverlapCircle(groundCheck.position, groundRadius, GroundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && canJump == true)
        {
            //rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);  // Aplica la fuerza de salto al Rigidbody2D.
        }

        playerAnimator.SetFloat("IsRunning", movement);

        movement = Input.GetAxisRaw("Horizontal");

        if (movement < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        else if (movement > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (hitTime <= 0)  // Si el tiempo de golpe ha terminado (el jugador no est� siendo empujado)
        {
            // L�gica de movimiento normal, el jugador se mueve a la izquierda o derecha seg�n la entrada.
            //rb.velocity = new Vector2(movement * speed, rb.velocity.y);
            transform.Translate(Time.deltaTime * (Vector2.right * movement) * speed);
        }
        else  // Si el jugador est� siendo golpeado
        {
            // L�gica de movimiento despu�s de un golpe. El jugador es empujado en la direcci�n del golpe.
            if (hitFromRight)
            {
                rb.velocity = new Vector2(-hitForceX, hitForceY);  // Empuja al jugador hacia la izquierda.
            }
            else if (!hitFromRight)
            {
                rb.velocity = new Vector2(hitForceX, hitForceY);  // Empuja al jugador hacia la derecha.
            }
            hitTime -= Time.deltaTime;  // Reduce el tiempo durante el cual el jugador es empujado.

            playerAnimator.SetTrigger("IsAttacked");  // Actualiza la animaci�n de movimiento del jugador en el Animator.

            // Si el jugador presiona la barra espaciadora y puede saltar, se le aplica una fuerza hacia arriba.

        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            playerAnimator.SetTrigger("IsAttacked");
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;  // Reduce la salud del jugador por la cantidad de da�o recibido.
        healthText.text = $"Health: {health}/{maxHealth}";  // Actualiza la UI para mostrar la nueva vida del jugador.

        // Configura y reproduce el sistema de part�culas para mostrar un efecto visual cuando el jugador recibe da�o.

        ParticleSystem.Burst burst = hit_ps.emission.GetBurst(0);  // Obtiene la configuraci�n de la emisi�n de part�culas en el �ndice 0 del sistema de part�culas del jugador (esto est� relacionado con el efecto visual de da�o).    
        ParticleSystem.MinMaxCurve count = burst.count;  // Obtiene la cantidad de part�culas que se reproducir�n en el efecto.

        count.constant = particleCount;  // Establece la cantidad constante de part�culas a reproducir. "particleCount" es una variable definida en el c�digo que controla cu�ntas part�culas se generar�n.
        burst.count = count;  // Aplica el nuevo valor de la cantidad de part�culas al objeto de emisi�n.

        hit_ps.emission.SetBurst(0, burst);  // Establece la configuraci�n de la emisi�n de part�culas con la nueva cantidad de part�culas en el sistema de part�culas.

        hit_ps.textureSheetAnimation.SetSprite(0, particleSprites[spriteIndex]);
        hit_ps.startColor = Color.red;
        hit_ps.Play();  // Inicia la reproducci�n del sistema de part�culas, mostrando el efecto visual del da�o.

        /*hit_ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        hit_ps.Emit((int)particleCount);*/

    }

    // Funci�n para agregar salud al jugador.
    public void AddHealth(float _health)
    {
        if (health + _health > maxHealth)  // Si la nueva salud supera la salud m�xima, ajusta la salud al valor m�ximo.
        {
            health = maxHealth;
            
        }
        else
        {
            health += _health;  // Si no, agrega la cantidad de salud especificada.
        }

        healthText.text = $"Health: {health}/{maxHealth}";  // Actualiza la UI con la nueva salud del jugador.

        ParticleSystem.Burst burst = hit_ps.emission.GetBurst(0);  // Obtiene la configuraci�n de la emisi�n de part�culas en el �ndice 0 del sistema de part�culas del jugador (esto est� relacionado con el efecto visual de da�o).    
        ParticleSystem.MinMaxCurve count = burst.count;  // Obtiene la cantidad de part�culas que se reproducir�n en el efecto.

        count.constant = particleCount;  // Establece la cantidad constante de part�culas a reproducir. "particleCount" es una variable definida en el c�digo que controla cu�ntas part�culas se generar�n.
        burst.count = count;  // Aplica el nuevo valor de la cantidad de part�culas al objeto de emisi�n.

        hit_ps.emission.SetBurst(0, burst);  // Establece la configuraci�n de la emisi�n de part�culas con la nueva cantidad de part�culas en el sistema de part�culas.

        hit_ps.textureSheetAnimation.SetSprite(0, particleSprites[spriteIndex]);
        hit_ps.startColor = Color.green;
        hit_ps.Play();
    }

    
}

