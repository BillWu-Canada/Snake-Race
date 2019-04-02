using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Snake : MonoBehaviour
{
	private Gameboard gameboard;
	private Food food;
	
	private LinkedList<Coordinate> snakeCoordinates; //coordinates of snake parts
	private int currentDirection = 0; //direction snake is heading
	private int currentSize = 4; //size of snake, starts at 4
	private float timeout = 0; //hacky variable used to limit times we update
	
	#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
	private bool isInputSensitive = true;
	#endif
	
	//dont use enums, becuase you can't get the ordinal easily in C#
	private int Up = 0;
	private int Right = 1;
	private int Down = 2;
	private int Left = 3;
	
	private Coordinate[] directions = new Coordinate[] {
		new Coordinate (-1, 0), // UP
		new Coordinate (0, 1),  // RIGHT
		new Coordinate (1, 0),  // DOWN
		new Coordinate (0, -1)  // LEFT
	};
	
	private void Awake ()
	{
		gameboard = GetComponent<Gameboard> ();
		food = GetComponent<Food> ();
	}
	
	#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
	private void OnEnable ()
	{
		TouchAndSwipeBehavior.OnSwipeDetected += HandleSwipeInput;
	}
	
	private void OnDisable ()
	{
		TouchAndSwipeBehavior.OnSwipeDetected -= HandleSwipeInput;
	}
	#endif
	
	private void Start ()
	{
		snakeCoordinates = new LinkedList<Coordinate> ();
		
		Initialize ();
	}
	
	//initialize again for every restart
	public void Initialize ()
	{
		snakeCoordinates.Clear ();
		
		int centerRow = gameboard.rows / 2;
		int centerColumn = gameboard.columns / 2;
		
		for (int i = 0; i < currentSize; ++i) {
			snakeCoordinates.AddLast (new Coordinate (centerRow + i, centerColumn));
			gameboard.SetCellActive (snakeCoordinates.Last.Value, true);
		}
		
		food.RemoveAllFood ();
		food.CreateFood ();
		
		currentDirection = 0;
	}
	
	//TODO remove update snake somewhere else since we are only checking every .5s
	private void Update ()
	{
		if (gameboard.isGameOver) {
			return;
		}
		
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
		if (isInputSensitive)
		{
			HandleKeyInput ();
		}
		#endif
		
		//really hacky stuff, should move this somehwere else so it only updates every .5 seconds and not 5 frames
		timeout++;
		if (timeout >= 5) {
			#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
			isInputSensitive = true;
			#endif
			UpdateSnake ();
			timeout = 0;
		}
	}
	
	#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
	private void HandleKeyInput ()
	{
		if (Input.GetKeyDown (KeyCode.A) && currentDirection != Right)
		{
			currentDirection = Left;
			isInputSensitive = false;
		}
		else if (Input.GetKeyDown (KeyCode.D) && currentDirection != Left)
		{
			currentDirection = Right;
			isInputSensitive = false;
		}
		else if (Input.GetKeyDown (KeyCode.W) && currentDirection != Down)
		{
			currentDirection = Up;
			isInputSensitive = false;
		}
		else if (Input.GetKeyDown (KeyCode.S) && currentDirection != Up)
		{
			currentDirection = Down;
			isInputSensitive = false;
		}
	}
	#endif
	
	#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
	private void HandleSwipeInput (Swipe swipe)
	{
		if (swipe == Swipe.Left && currentDirection != Right)
		{
			currentDirection = Left;
		}
		else if (swipe == Swipe.Right && currentDirection != Left)
		{
			currentDirection = Right;
		}
		else if (swipe == Swipe.Up && currentDirection != Down)
		{
			currentDirection = Up;
		}
		else if (swipe == Swipe.Down && currentDirection != Up)
		{
			currentDirection = Down;
		}
	}
	#endif
	
	//updates the snake TODO: color/texture changes for food and snake
	private void UpdateSnake ()
	{
		Coordinate head = snakeCoordinates.First.Value;
		Coordinate newCoordinate = new Coordinate (head.x + directions [currentDirection].x, head.y + directions [currentDirection].y);
		
		ValidatePosition (newCoordinate);
		
		if (IsSnakeIntersectingWith (newCoordinate)) {
			gameboard.SetGameOver (true);
			return;
		}
		
		snakeCoordinates.AddFirst (newCoordinate);
		
		if (food.IsFoodAt (newCoordinate)) {
			if (food.GetFoodTypeAt  (newCoordinate) == SnakeFood.FoodType.FOOD) {
				food.CreateFood (); //create food
			}
			food.RemoveFood (newCoordinate); //remove last food
		} else {
			gameboard.SetCellActive (newCoordinate, true);
			gameboard.SetCellActive (snakeCoordinates.Last.Value, false);
			snakeCoordinates.RemoveLast ();
		}
	}
	
	//Makes it so the snake loops forever
	private void ValidatePosition (Coordinate coordinate)
	{
		coordinate.x %= gameboard.rows;
		coordinate.y %= gameboard.columns;
		
		if (coordinate.x < 0) {
			coordinate.x += gameboard.rows;
		}
		
		if (coordinate.y < 0) {
			coordinate.y += gameboard.columns;
		}
	}
	
	//get coordiantes of snake
	public LinkedList<Coordinate> GetSnakeCoordinates ()
	{
		return snakeCoordinates;
	}
	
	//check if snake intersects with coordinates TODO: gotta check if snake hits another player's snake
	public bool IsSnakeIntersectingWith (Coordinate coordinate)
	{
		if (snakeCoordinates != null) {
			foreach (Coordinate snake in snakeCoordinates) {
				if (coordinate.x == snake.x && coordinate.y == snake.y) {
					return true;
				}
			}
		}
		return false;
	}
}