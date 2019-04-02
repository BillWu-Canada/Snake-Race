using UnityEngine;
using System.Collections;

//structure for coordinates
public class Coordinate
{
	public int x { get; set; }

	public int y { get; set; }
	
	public Coordinate (int pX = 0, int pY = 0)
	{
		x = pX;
		y = pY;
	}
}

public class Gameboard : MonoBehaviour
{
	//inspector variables (TODO: figure why this is not showing up in the inspector)
	public int rows { get; private set; }

	public int columns { get; private set; }

	public const int SCALE_FACTOR = 16; //size of cell
	
	private GameObject cellObject;
	private GameObject[,] cells; //2D array of cellObject
	
	public bool isGameOver { get; private set; } //set if its game over
	
	private void Awake ()
	{
		//set rows based on height TODO use fixed rows and columns, and calculate a scale factor
		rows = Screen.height / SCALE_FACTOR;
		columns = Screen.width / SCALE_FACTOR;
		
		//set camera orientation
		Camera.main.orthographicSize = rows * SCALE_FACTOR / 2;
		
		//get cell object
		cellObject = Resources.Load<GameObject> ("Cell");
		cellObject.transform.localScale = new Vector3 (SCALE_FACTOR, SCALE_FACTOR, 1);
		cellObject.SetActive (false);
		
		//calculate offsets
		float offsetX = (columns - 1) * (SCALE_FACTOR / 2);
		float offsetY = (rows - 1) * (SCALE_FACTOR / 2);
		
		//add cells to array
		cells = new GameObject[rows, columns];
		for (int row = 0; row < rows; row++) {
			for (int column = 0; column < columns; column++) {
				//make clone
				GameObject cell = Object.Instantiate (cellObject) as GameObject;
				
				//offset cell vector
				Vector3 vector = cell.transform.position;
				vector.x = column * SCALE_FACTOR - offsetX;
				vector.y = offsetY - row * SCALE_FACTOR;
				
				//set cell in array
				cell.transform.position = vector;
				cells [row, column] = cell;
			}
		}
		
		//set game variables
		SetGameOver (false);
	}
	
	//set game as gameover
	public void SetGameOver (bool pIsGameOver)
	{
		isGameOver = pIsGameOver;
	}
	
	// get cell coordinates
	public GameObject GetCell (Coordinate coordinate)
	{
		return cells [coordinate.x, coordinate.y];
	}
	
	//set cell active
	public void SetCellActive (Coordinate coordinate, bool isActive)
	{
		cells [coordinate.x, coordinate.y].SetActive (isActive);
	}
	
	//change cell color
	public void setCellColor (Coordinate coordinate, Color color)
	{
		cells [coordinate.x, coordinate.y].GetComponent<SpriteRenderer> ().color = color;
	}
}

