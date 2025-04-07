using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Private Variables")]  // Título para agrupar las variables privadas en el inspector de Unity.
    [Space]

    [Tooltip("La vida máxima que el jugador puede tener (se puede modificar desde Unity).")]
    [SerializeField] private float maxHealth = 100;
    [Tooltip("Referencia al Rigidbody2D del jugador, usado para aplicar física y movimiento.")]
    [SerializeField] private Rigidbody2D rb;
    [Tooltip("Un transform que indica la posición donde se verifica si el jugador está tocando el suelo.")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("El radio de la zona de verificación del suelo (es un círculo).")]
    [SerializeField] private float groundRadius;
    [Tooltip("La capa en la que se consideran los objetos \"suelo\".")]
    [SerializeField] private LayerMask GroundLayer;
    [Tooltip("La capa en la que se consideran los objetos \"barro\", que afectan al movimiento del jugador.")]
    [SerializeField] private LayerMask mudLayer;
    [Tooltip("El sistema de partículas que se reproducirá cuando el jugador reciba daño.")]
    [SerializeField] private ParticleSystem hit_ps;
    [Tooltip("El componente Animator para controlar las animaciones del jugador.")]
    [SerializeField] public Animator playerAnimator;

    private float movement;

    [Header("Public Variables")]  // Título para agrupar las variables públicas que pueden ser modificadas desde otros scripts o el inspector.
    [Space]

    [Tooltip("La vida actual del jugador.")]
    public float health;
    [Tooltip("La velocidad de movimiento del jugador.")]
    public float speed;
    [Tooltip("La fuerza de salto del jugador.")]
    public float jumpForce;
    [Tooltip("Determina si el jugador puede saltar (está en el suelo o no en barro).")]
    public bool canJump;
    [Tooltip("El tiempo que el jugador estará siendo empujado después de recibir un golpe.")]
    public float hitTime;
    [Tooltip("La fuerza de empuje en el eje horizontal cuando el jugador recibe un golpe.")]
    public float hitForceX;
    [Tooltip("La fuerza de empuje en el eje vertical cuando el jugador recibe un golpe.")]
    public float hitForceY;
    [Tooltip("Determina si el golpe fue desde la derecha o no (usado para direccionar el empuje).")]
    public bool hitFromRight;
    [Tooltip("Texto en la UI que muestra la vida del jugador.")]
    public TextMeshProUGUI healthText;
    [Tooltip("La cantidad de partículas que se generarán cuando el jugador reciba daño.")]
    public float particleCount;

    
    [SerializeField] private Sprite[] particleSprites;
    public int spriteIndex;

    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Obtiene el componente Rigidbody2D del jugador.
        playerAnimator = GetComponent<Animator>();  // Obtiene el componente Animator para controlar animaciones.
        hit_ps = GetComponentInChildren<ParticleSystem>();  // Obtiene el sistema de partículas del jugador (hijo del objeto).
        
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

        if (hitTime <= 0)  // Si el tiempo de golpe ha terminado (el jugador no está siendo empujado)
        {
            // Lógica de movimiento normal, el jugador se mueve a la izquierda o derecha según la entrada.
            //rb.velocity = new Vector2(movement * speed, rb.velocity.y);
            transform.Translate(Time.deltaTime * (Vector2.right * movement) * speed);
        }
        else  // Si el jugador está siendo golpeado
        {
            // Lógica de movimiento después de un golpe. El jugador es empujado en la dirección del golpe.
            if (hitFromRight)
            {
                rb.velocity = new Vector2(-hitForceX, hitForceY);  // Empuja al jugador hacia la izquierda.
            }
            else if (!hitFromRight)
            {
                rb.velocity = new Vector2(hitForceX, hitForceY);  // Empuja al jugador hacia la derecha.
            }
            hitTime -= Time.deltaTime;  // Reduce el tiempo durante el cual el jugador es empujado.

            playerAnimator.SetTrigger("IsAttacked");  // Actualiza la animación de movimiento del jugador en el Animator.

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
        health -= damage;  // Reduce la salud del jugador por la cantidad de daño recibido.
        healthText.text = $"Health: {health}/{maxHealth}";  // Actualiza la UI para mostrar la nueva vida del jugador.

        // Configura y reproduce el sistema de partículas para mostrar un efecto visual cuando el jugador recibe daño.

        ParticleSystem.Burst burst = hit_ps.emission.GetBurst(0);  // Obtiene la configuración de la emisión de partículas en el índice 0 del sistema de partículas del jugador (esto está relacionado con el efecto visual de daño).    
        ParticleSystem.MinMaxCurve count = burst.count;  // Obtiene la cantidad de partículas que se reproducirán en el efecto.

        count.constant = particleCount;  // Establece la cantidad constante de partículas a reproducir. "particleCount" es una variable definida en el código que controla cuántas partículas se generarán.
        burst.count = count;  // Aplica el nuevo valor de la cantidad de partículas al objeto de emisión.

        hit_ps.emission.SetBurst(0, burst);  // Establece la configuración de la emisión de partículas con la nueva cantidad de partículas en el sistema de partículas.

        hit_ps.textureSheetAnimation.SetSprite(0, particleSprites[spriteIndex]);
        hit_ps.startColor = Color.red;
        hit_ps.Play();  // Inicia la reproducción del sistema de partículas, mostrando el efecto visual del daño.

        /*hit_ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        hit_ps.Emit((int)particleCount);*/

    }

    // Función para agregar salud al jugador.
    public void AddHealth(float _health)
    {
        if (health + _health > maxHealth)  // Si la nueva salud supera la salud máxima, ajusta la salud al valor máximo.
        {
            health = maxHealth;
            
        }
        else
        {
            health += _health;  // Si no, agrega la cantidad de salud especificada.
        }

        healthText.text = $"Health: {health}/{maxHealth}";  // Actualiza la UI con la nueva salud del jugador.

        ParticleSystem.Burst burst = hit_ps.emission.GetBurst(0);  // Obtiene la configuración de la emisión de partículas en el índice 0 del sistema de partículas del jugador (esto está relacionado con el efecto visual de daño).    
        ParticleSystem.MinMaxCurve count = burst.count;  // Obtiene la cantidad de partículas que se reproducirán en el efecto.

        count.constant = particleCount;  // Establece la cantidad constante de partículas a reproducir. "particleCount" es una variable definida en el código que controla cuántas partículas se generarán.
        burst.count = count;  // Aplica el nuevo valor de la cantidad de partículas al objeto de emisión.

        hit_ps.emission.SetBurst(0, burst);  // Establece la configuración de la emisión de partículas con la nueva cantidad de partículas en el sistema de partículas.

        hit_ps.textureSheetAnimation.SetSprite(0, particleSprites[spriteIndex]);
        hit_ps.startColor = Color.green;
        hit_ps.Play();
    }

    
}

