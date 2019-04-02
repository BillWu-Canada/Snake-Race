using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//snake food structure
public class SnakeFood
{
	public enum FoodType { //types of food
		FOOD,
		DEADSNAKE
	}

	public Coordinate foodCoordinate { get; set; }
	public FoodType foodType { get; set; }
	
	public SnakeFood (Coordinate pCoordinate, FoodType pFood = FoodType.FOOD)
	{
		foodCoordinate = pCoordinate;
		foodType = pFood;
	}
}

public class Food : MonoBehaviour
{
	private Gameboard gameboard;
	private Snake snake;
	
	private System.Random random; //for random generation
	private LinkedList<SnakeFood> foods; //locations of food

	//initialize everything
	private void Awake ()
	{
		//initialize components
		gameboard = GetComponent<Gameboard> ();
		snake = GetComponent<Snake> ();
		
		//initialize variables
		random = new System.Random ();
		foods = new LinkedList<SnakeFood> ();
	}
	
	//randomly generate food (for single player)
	public void CreateFood ()
	{
		Coordinate newFoodLocation = new Coordinate ();
		
		//keep randomly generating food if it overlaps snake
		do {
			newFoodLocation.x = random.Next (gameboard.rows);
			newFoodLocation.y = random.Next (gameboard.columns);
		} while (snake.IsSnakeIntersectingWith (newFoodLocation) || IsFoodAt (newFoodLocation));
		
		//add food to list
		foods.AddLast (new SnakeFood(newFoodLocation, SnakeFood.FoodType.FOOD));

		//activate that cell
		gameboard.SetCellActive (newFoodLocation, true);
	}
	
	//generate food at location
	public void CreateDeadSnakeFood (Coordinate pFoodLocation, SnakeFood.FoodType foodtype)
	{
		//add food to list and activate that cell
		foods.AddLast (new SnakeFood(pFoodLocation, SnakeFood.FoodType.FOOD));
		gameboard.SetCellActive (pFoodLocation, true);
	}
	
	//remove food at location
	public void RemoveFood (Coordinate pFoodLocation)
	{
		foreach (SnakeFood food in foods) {
			if (food.foodCoordinate.x == pFoodLocation.x && food.foodCoordinate.y == pFoodLocation.y) {
				foods.Remove (food);
				break;
			}
		}
	}
	
	//remove all food (for restart or out of sync)
	public void RemoveAllFood ()
	{
		//deactivate cells
		foreach (SnakeFood food in foods) {
			gameboard.SetCellActive (food.foodCoordinate, false);
		}
		//clear everything
		foods.Clear ();
	}
	
	//check if food can be found at location
	public bool IsFoodAt (Coordinate pFoodLocation)
	{
		foreach (SnakeFood food in foods) {
			if (food.foodCoordinate.x == pFoodLocation.x && food.foodCoordinate.y == pFoodLocation.y) {
				return true;
			}
		}
		return false;
	}

	//get Foodtype at location
	public SnakeFood.FoodType GetFoodTypeAt (Coordinate pFoodLocation)
	{
		foreach (SnakeFood food in foods) {
			if (food.foodCoordinate.x == pFoodLocation.x && food.foodCoordinate.y == pFoodLocation.y) {
				return food.foodType;
			}
		}
		return SnakeFood.FoodType.FOOD;
	}
}
