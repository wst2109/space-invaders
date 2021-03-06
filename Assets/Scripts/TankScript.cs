﻿using UnityEngine;
using System.Collections;

public class TankScript : MonoBehaviour 
{
	public float leftBound;
	public float rightBound;

	public int lives;

	// death animation
	public Sprite normalSprite;
	public Sprite death1;
	public Sprite death2;
	public Sprite death3;

	public float animSpeed; // frames per sec
	public int animCycles; // number of times we cycle through animation

	private bool dying;
	private int cel;
	private float celPeriod;
	private float celTime; // secs in the current cel
	private int cycleCount; // counter for cycles

	private MoveScript moveScript;
	private WeaponScript weaponScript;
	private SpriteRenderer spriteRenderer;

	private Vector2 startPos;
	private float startSpeed;

	public void Start()
	{
		moveScript = GetComponent<MoveScript>();
		weaponScript = GetComponentInChildren<WeaponScript>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		dying = false;
		cel = 1;
		celPeriod = 1 / animSpeed;
		celTime = 0;
		cycleCount = 0;

		if (leftBound > rightBound)
		{
			Debug.LogError("left bound greater than right bound!");
		}

		GameManager.Instance.AdjustLives(lives);

		startPos = transform.position;
		startSpeed = moveScript.speed;
	}

	public void Update()
	{
		if (!dying)
		{
			HandleInput();
		}
		else
		{
			UpdateDeathAnimation();
		}

		// make sure that tanks stays on screen
		transform.position = new Vector2(Mathf.Clamp(transform.position.x, leftBound, rightBound),
		                                 			 transform.position.y);
	}

	public void OnCollisionEnter2D(Collision2D coll)
	{
		Destroy(coll.gameObject);

		lives--;

		dying = true;
		moveScript.speed = 0;
		SoundEffectsHelper.Instance.PlayPlayerDeathSound();

		GameManager.Instance.AdjustLives(lives);
	}

	private void HandleInput()
	{
		// movement
		if (Input.GetKey("left"))
		{
			moveScript.direction = new Vector2(-1, 0);
		}
		else if (Input.GetKey("right"))
		{
			moveScript.direction = new Vector2(1,0);
		}
		else
		{
			moveScript.direction = Vector2.zero;
		}

		// firing
		if (Input.GetKey("space"))
		{
			weaponScript.Fire(new Vector2(0, 1));
		}
	}

	private void UpdateDeathAnimation()
	{
		celTime -= Time.deltaTime;

		if (cycleCount == animCycles)
		{
			// reset if we still have lives, otherwise end level
			if (lives > 0)
			{
				Reset();
			}
			else
			{
				Application.LoadLevel("GameOver");
			}
			return;
		}

		if (celTime <= 0)
		{
			switch (cel)
			{
			case 1:
				spriteRenderer.sprite = death1;
				cel++;
				break;
				
			case 2:
				spriteRenderer.sprite = death2;
				cel++;
				break;
				
			case 3:
				spriteRenderer.sprite = death3;
				cycleCount++;
				cel = 1;
				break;
				
			default:
				Debug.LogError("Unexpected cel number!");
				break;
			}

			celTime = celPeriod;
		}
	}

	private void Reset()
	{
		dying = false;
		spriteRenderer.sprite = normalSprite;
		transform.position = startPos;
		moveScript.speed = startSpeed;
		cycleCount = 0;
		celTime = 0;
		cel = 1;
	}
}
