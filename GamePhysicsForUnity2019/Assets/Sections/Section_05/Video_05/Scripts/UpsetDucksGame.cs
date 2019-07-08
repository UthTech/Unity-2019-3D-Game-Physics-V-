﻿using RMC.UnityGamePhysics.Shared;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace RMC.UnityGamePhysics.Sections.Section05.Video05
{
	public class UpsetDucksGame : MonoBehaviour
	{
		/// <summary>
		/// Setup "Singleton" Design Pattern
		/// See http://bit.ly/Unity_Singleton
		/// </summary>
		private static UpsetDucksGame _instance;
		public static UpsetDucksGame Instance { get { return _instance; } }

		public int Score
		{
			get
			{
				return _score;
			}
			set
			{
				_score = value;
				if (UpsetDucksUI.Instance != null)
				{
					UpsetDucksUI.Instance.ShowScore(_score);
				}
				
			}
		}

		public int Asteroids
		{
			get
			{
				return _asteroids;
			}
			set
			{
				_asteroids = value;
				if (UpsetDucksUI.Instance != null)
				{
					UpsetDucksUI.Instance.ShowAsteroids(_asteroids);
				}
			}
		}

		[SerializeField]
		private GameObject _explosionPrefab = null;

		[SerializeField]
		private GameObject _worldItemParent = null;

		public List<WorldItem> _worldItems = new List<WorldItem>();

		private int _asteroids = 0;
		private int _score = 0;
		private bool _isGameOver = false;
		private int _upsetDuckCount = 0;
		private Asteroid _currentAsteroid = null;

		protected void Awake()
		{
			_instance = this;
		}

		protected void Start()
		{
			//Default 0.005 - Change to make turns end more quickly
			Physics.sleepThreshold = 0.05f;

			// Reset values
			_upsetDuckCount = 0;
			_isGameOver = false;
			_currentAsteroid = null;
			Score = 0;
			Asteroids = 3;

			// Create new list of worldItems
			_worldItems = _worldItemParent.GetComponentsInChildren<WorldItem>().ToList();
			foreach (WorldItem worldItem in _worldItems)
			{
				if (worldItem.gameObject.tag == UpsetDucksConstants.UpsetDuckTag)
				{
					// Count how many upsetducks so we know how many we must 'hit'
					_upsetDuckCount++;
				}
			}

			// Start the catapult
			AddAsteroid();
		}

		protected void Update()
		{
			if (_isGameOver)
			{
				return;
			}

			if (_currentAsteroid != null)
			{
				if (_currentAsteroid.IsReleased && 
					_currentAsteroid.Rigidbody2D.IsSleeping())
				{
					_currentAsteroid = null;
					AddAsteroid();
				}
			}

			foreach (WorldItem worldItem in _worldItems)
			{
				if (worldItem.gameObject.tag == UpsetDucksConstants.UpsetDuckTag)
				{
					
					if (worldItem.IsAlive && worldItem.Health <= 0)
					{
						worldItem.IsAlive = false;
						Score += 1;
					}
				}
			}

			if (Score >= _upsetDuckCount)
			{
				if (UpsetDucksUI.Instance != null)
				{
					UpsetDucksUI.Instance.ShowResult(true);
				}
				_isGameOver = true;
			}
		}

		private void AddAsteroid()
		{
			if (Asteroids > 0)
			{
				Asteroids -= 1;
				if (Catapult.Instance != null)
				{
					_currentAsteroid = Catapult.Instance.AddAsteroid();
				}
			}
			else
			{
				UpsetDucksUI.Instance.ShowResult(false);
				_isGameOver = true;
			}
		}

		public void DestroyCrate(Crate crate)
		{
			SoundManager.Instance.PlayAudioClip(UpsetDucksConstants.ExplosionSound);

			GameObject explosion = Instantiate(_explosionPrefab);
			explosion.transform.position = crate.transform.position;

			_worldItems.Remove(crate.gameObject.GetComponent<WorldItem>());

			StartCoroutine(DestroyGameObjectAfterXSeconds(crate.gameObject, 0));
			StartCoroutine(DestroyGameObjectAfterXSeconds(explosion.gameObject, 0.25f));
		}

		private IEnumerator DestroyGameObjectAfterXSeconds(GameObject go, float seconds)
		{
			yield return new WaitForSeconds(seconds);
			Destroy(go);
		}
	}
}