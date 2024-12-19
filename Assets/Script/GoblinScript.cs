using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinScript : MonoBehaviour
{
    public float suspicionLevel;
    public bool isSuspicious;
    private bool wasSuspicious = false;
    public float suspicionThreshold = 3f;
    public BoxCollider2D suspicionZone;
    // [SerializeField] GameObject player;
    Vector2 lastKnownPlayerPosition;
    public float speed = 3.0f;


    Animator animator;
    public Transform[] waypoints; // Array untuk menyimpan titik-titik tujuan NPC
    public int currentWaypoint = 0; // Indeks titik tujuan saat ini
    private float idleTimer = 0f; // Penghitung waktu untuk perilaku idle
    private float idleDuration = 3f; // Durasi idle dalam detik
    public bool isIdle = true; // Status apakah NPC sedang idle atau tidak


    //public GameObject baby;

    // Keputusan untuk memakai tangga/lift ChangeFloor
    // Keputusan untuk memakai tangga/lift ChangeFloor



    void Start()
    {
        animator = GetComponent<Animator>();

    }


    void Update()
    {
        NPCSus();
        Animator();

    }

    void NPCSus()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Meningkatkan tingkat kecurigaan jika pemain berada di SusZone
            if (suspicionZone.bounds.Contains(player.transform.position))
            {
                suspicionLevel += Time.deltaTime;
                lastKnownPlayerPosition = player.transform.position;
            }
            else
            {
                // Mengurangi tingkat kecurigaan jika pemain tidak berada di SusZone
                suspicionLevel -= Time.deltaTime;
            }

            // Memastikan tingkat kecurigaan berada di antara 0 dan suspicionThreshold
            suspicionLevel = Mathf.Clamp(suspicionLevel, 0, suspicionThreshold);

            // NPC menjadi curiga jika tingkat kecurigaan mencapai ambang batas
            isSuspicious = suspicionLevel >= suspicionThreshold;

            // Jika NPC curiga, bergerak ke posisi terakhir player
            if (isSuspicious)
            {
                Vector2 direction = new Vector2(lastKnownPlayerPosition.x - transform.position.x, 0).normalized;
                GetComponent<Rigidbody2D>().velocity = direction * speed * 1.15f;
                if (direction.x < 0)
                {
                    GetComponent<SpriteRenderer>().flipX = true;
                }
                else if (direction.x > 0)
                {
                    GetComponent<SpriteRenderer>().flipX = false;
                }
            }
        }
        wasSuspicious = isSuspicious;
        if (!isSuspicious) { patrolling(); }
    }

    void patrolling()
    {
        // Jika NPC sedang idle
        if (isIdle)
        {
            idleTimer += Time.deltaTime; // Tambahkan waktu ke penghitung idle

            // Jika durasi idle telah tercapai
            if (idleTimer >= idleDuration)
            {
                isIdle = false; // Atur status NPC menjadi tidak idle
                idleTimer = 0f; // Reset penghitung idle
            }
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (!isIdle) // Jika NPC tidak idle
        {
            // Dapatkan posisi titik tujuan saat ini
            Vector2 targetPosition = waypoints[currentWaypoint].position;

            // Hitung arah pergerakan NPC
            Vector2 direction = targetPosition - (Vector2)transform.position;

            // Ubah arah menghadap NPC berdasarkan arah pergerakan
            if (direction.x > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;// Menghadap ke kanan
            }
            else if (direction.x < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;// Menghadap ke kiri
            }

            transform.position = new Vector2(Mathf.MoveTowards(transform.position.x, targetPosition.x, speed * Time.deltaTime), transform.position.y);
            //GetComponent<Rigidbody2D>().velocity = direction * speed;

            // Jika NPC telah mencapai titik tujuan
            if (Vector2.Distance(transform.position, targetPosition) < 1f)
            {
                isIdle = true; // Atur status NPC menjadi idle
                currentWaypoint++; // Pindah ke titik tujuan berikutnya

                // Jika telah mencapai titik tujuan terakhir, kembali ke titik awal
                if (currentWaypoint >= waypoints.Length)
                {
                    currentWaypoint = 0;
                }
            }
        }
    }

    void Animator()
    {
        // Jika NPC sedang idle
        if (isIdle && !isSuspicious)
        {
            animator.SetBool("isIdle", true);

        }
        else // Jika NPC tidak idle (sedang berjalan)
        {
            animator.SetBool("isIdle", false);
        }
    }
}