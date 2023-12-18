using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapGrid : MonoBehaviour
{

    public int xDimensionSize;
    public int yDimensionSize;
    public int rooms;
    private List<Vector3Int> roomMidPoints;
    private List<Vector3Int> obstacleSpawns;
    private List<Vector3Int> meleeEnemySpawns;
    public Tilemap grid;
    public Tile[] floor; //0 blue floor, 1 wall, 2 red floor, 3 hole
    public Tile[] enemies; //0 Melee, 1 Ranged

    // Start is called before the first frame update
    void Start()
    {
        roomMidPoints = new List<Vector3Int>();
        obstacleSpawns = new List<Vector3Int>();
        meleeEnemySpawns = new List<Vector3Int>();
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        //GENERATE A NEW MAP IF RETURN IS PRESSED
        if(Input.GetKeyDown(KeyCode.Return)){
            roomMidPoints = new List<Vector3Int>(); //Must reset roomMidPoints list
            obstacleSpawns = new List<Vector3Int>(); //Must reset obstacleSpawns list
            meleeEnemySpawns = new List<Vector3Int>(); //Must reset meleeEnemySpawns list
            GenerateMap();
        }

        //QUIT APPLICATION IF ESCAPE IS PRESSED
        if(Input.GetKeyDown(KeyCode.Escape)){
            Application.Quit();
        }
    }

    void GenerateMap()
    {
        grid.ClearAllTiles();
        //GeneratePath();
        //GenerateStartingRoom();
        GenerateRooms();
        GenerateRoomConnections();
        GenerateFillerTiles();
        //GenerateHole(10, 10);
        //GenerateCover(10, 10);
        //GenerateMeleeEnemy(10, 10);
        //GenerateRangedEnemy(20, 20);
    }

    void GenerateFillerTiles()
    {
        //For loop fills tiles around generated path
        for(int x = 0; x<xDimensionSize; x++){
            for(int y = 0; y<yDimensionSize; y++){
                if(!grid.HasTile(new Vector3Int(x, y, 0))){
                     //grid.SetTile(new Vector3Int(x, y, 0), floor[Random.Range(0, floor.Length - 1)]);
                     grid.SetTile(new Vector3Int(x, y, 0), floor[1]);
                }
            }
        }
    }

    //Starting room should be largest room; also working on making it look more unique
    //To make it easy, lots of space for kiting + ONLY melee enemies
    //Introduce ranged enemies and combinations of the two later on
    int GenerateStartingRoom()
    {
        Debug.Log("Generating starting room!");

        //Large starting room dimensions
        int dim1 = 50;
        int dim2 = 50;

        //Make a list of points to calculate room midpoints later
        List<Vector3Int> roomCords = new List<Vector3Int>();

        //While loop until base room rectangle is generated
        while(true){

            //Pick a random X,Y coordinate
            int randX = UnityEngine.Random.Range(0, xDimensionSize);
            int randY = UnityEngine.Random.Range(0, yDimensionSize);

            if(randX < xDimensionSize - 49 && randY < yDimensionSize - 49 && !grid.HasTile(new Vector3Int(randX, randY, 0))){
                for(int x = 0; x<dim1; x++){
                    for(int y = 0; y<dim2; y++){
                        grid.SetTile(new Vector3Int(x + randX, y + randY, 0), floor[2]);

                        //Add tile to roomCords
                        roomCords.Add(new Vector3Int(x + randX, y + randY, 0));
                    }
                }

                //Find the average of roomCords.X & Y and then avearage for room midpoint
                //and add to roomMidPoints
                int totalX = 0;
                int totalY = 0;
                foreach(Vector3Int coord in roomCords){
                    totalX += coord.x;
                    totalY += coord.y;
                }
                roomMidPoints.Add(new Vector3Int(totalX/roomCords.Count, totalY/roomCords.Count, 0));
                break; //exit while loop if since room has been generated
            }
        }

        //Three possibilties for starting room type
        int randChoice = UnityEngine.Random.Range(0,3); 

        if(randChoice == 0){ //DONUT ROOM
            for(int x = 0; x<21; x++){
                for(int y = 0; y<20; y++){
                    Vector3Int tileToChange = new Vector3Int(roomMidPoints[0].x - 10 + x, roomMidPoints[0].y - 10 + y, 0);
                    grid.SetTile(tileToChange, floor[3]);
                    
                    //REMOVE COORDS FROM ROOMTILES SO THAT ENEMIES DON'T SPAWN IN HOLE
                    foreach(Vector3Int coord in roomCords){
                        if(coord.x == tileToChange.x && coord.y == tileToChange.y){
                            roomCords.Remove(coord);
                            break;
                        }
                    }
                }
            }
        }else if(randChoice == 1){ //CROSS ROOM

            //Bottom Left wall fill
            for(int x = 0; x<13; x++){
                for(int y = 0; y<13; y++){
                    grid.SetTile(new Vector3Int(roomMidPoints[0].x - 25 + x, roomMidPoints[0].y - 25 + y, 0), null);
                }
            }

            //Top Left wall fill
            for(int x = 0; x<13; x++){
                for(int y = 0; y<14; y++){
                    grid.SetTile(new Vector3Int(roomMidPoints[0].x - 25 + x, roomMidPoints[0].y + 12 + y, 0), null);
                }
            }

            //Bottom Right wall fill
            for(int x = 0; x<14; x++){
                for(int y = 0; y<13; y++){
                    grid.SetTile(new Vector3Int(roomMidPoints[0].x + 12 + x, roomMidPoints[0].y - 25 + y, 0), null);
                }
            }

            //Top Right wall fill
            for(int x = 0; x<14; x++){
                for(int y = 0; y<14; y++){
                    grid.SetTile(new Vector3Int(roomMidPoints[0].x + 12 + x, roomMidPoints[0].y + 12 + y, 0), null);
                }
            }

        } //If randChoice is 2 don't do anything and just have a regular rectangular room
        
        //Spawn melee enemies randomly within the bounds of the room
        int enemiesToSpawn = 3;
        while(enemiesToSpawn != 0){

            int randNum = UnityEngine.Random.Range(0, roomCords.Count);
            Vector3Int randCoord = roomCords[randNum];

            //Check that there is enough space to spawn a 2x2 enemy
            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(randCoord.x, randCoord.y, 0)),
            (Tile)grid.GetTile(new Vector3Int(randCoord.x + 1, randCoord.y - 1, 0))) &&
            IsSameTile((Tile)grid.GetTile(new Vector3Int(randCoord.x, randCoord.y, 0)), floor[2])){
                GenerateMeleeEnemy(randCoord.x, randCoord.y);
                enemiesToSpawn--;
            }
            
        }
        
        Debug.Log("Room Generated!");
        //Return -1 to communicate to rooms variable
        return -1;
    }

    void GenerateRooms()
    {
        //Generate starting room and decrement roomsCount since GenerateStartingRoom returns -1
        int roomsCount = rooms + GenerateStartingRoom();

        while(roomsCount != 0){

            //Debug.Log("Attempting Room Generation...");

            //Pick a random X,Y coordinate
            int randX = UnityEngine.Random.Range(0, xDimensionSize);
            int randY = UnityEngine.Random.Range(0, yDimensionSize);

            //Assign room dimensions between 5-20 Tiles
            int dim1 = UnityEngine.Random.Range(5, 21);
            int dim2 = UnityEngine.Random.Range(5,21);

            //Make a list of points to calculate room midpoints later
            List<Vector3Int> roomCords = new List<Vector3Int>();

            //Check to make sure there is enough space to generate 20x20 room
            if(randX < xDimensionSize - 19 && randY < yDimensionSize - 19 && !grid.HasTile(new Vector3Int(randX, randY, 0))){
                for(int x = 0; x<dim1; x++){
                    for(int y = 0; y<dim2; y++){

                        if(!grid.HasTile(new Vector3Int(x + randX, y + randY, 0))){
                            grid.SetTile(new Vector3Int(x + randX, y + randY, 0), floor[0]);
                        }

                        //Add Tile to roomCords
                        roomCords.Add(new Vector3Int(x + randX, y + randY, 0));
                    }
                }

                //Find the average of roomCords.X & Y and then avearage for room midpoint
                //and add to roomMidPoints
                //USE ROOMCORDS TO GENERATE OBSTACLES
                //USE RANDOM TO CHOOSE OBSTACLE TYPE
                int totalX = 0;
                int totalY = 0;
                int obstacleType = UnityEngine.Random.Range(0,2); //0 is hole room, 1 is wall room
                foreach(Vector3Int coord in roomCords){
                    int obstacleSpawnChance = UnityEngine.Random.Range(0, 100); // 1/100 chance to spawn an obstacle

                    //Check that there is enough space to spawn otherwise do nothing
                    //Check that coords are within the boundaries of grid's xy dimension sizes
                    //Also check that obstacle is spawning in blue floor tiles
                    if(obstacleSpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(coord.x - 1, coord.y, 0)), 
                    (Tile)grid.GetTile(new Vector3Int(coord.x + 2, coord.y - 3, 0))) &&
                    IsSameTile((Tile)grid.GetTile(new Vector3Int(coord.x, coord.y, 0)), floor[0]) && 
                    4 < coord.x && coord.x < xDimensionSize - 4 &&
                    4 < coord.y && coord.y < yDimensionSize - 4){
                        if(obstacleType == 0){
                            GenerateHole(coord.x, coord.y);
                            obstacleSpawns.Add(new Vector3Int(coord.x, coord.y, 0));
                        }else{
                            GenerateCover(coord.x, coord.y);
                            obstacleSpawns.Add(new Vector3Int(coord.x, coord.y, 0));
                        }
                    }

                    totalX += coord.x;
                    totalY += coord.y;
                }
                Vector3Int midPoint = new Vector3Int(totalX/roomCords.Count, totalY/roomCords.Count, 0);
                roomMidPoints.Add(midPoint);

                //USE ROOMCORDS TO GENERATE MELEE ENEMIES
                foreach(Vector3Int coord in roomCords){
                    int meleeEnemySpawnChance = UnityEngine.Random.Range(0, 100); // 1/100 chance to spawn a melee enemy

                    //Check that there is enough space to spawn otherwise do nothing
                    //Check that coords are within the boundaries of grid's xy dimension sizes
                    //Also check to see that the coordinate is a floor tile
                    if(meleeEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(coord.x, coord.y, 0)), 
                    (Tile)grid.GetTile(new Vector3Int(coord.x + 1, coord.y - 1, 0))) && 
                    IsSameTile((Tile)grid.GetTile(new Vector3Int(coord.x, coord.y, 0)), floor[0])&& 
                    2 < coord.x && coord.x < xDimensionSize - 2 &&
                    2 < coord.y && coord.y < yDimensionSize - 2){
                        GenerateMeleeEnemy(coord.x, coord.y);
                        meleeEnemySpawns.Add(new Vector3Int(coord.x, coord.y, 0));
                    }
                }

                //USE OBSTACLE SPAWNS TO GENERATE RANGED ENEMIES
                foreach(Vector3Int obstacle in obstacleSpawns){
                    int rangedEnemySpawnChance = UnityEngine.Random.Range(0, 10); // 1/10 spawn chance
                    int spawnPosition = UnityEngine.Random.Range(0, 4); // 0 RTOP, 1 LTOP, 2 RBOT, 3 LBOT

                    if(spawnPosition == 0){ //RTOP
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x + 2, obstacle.y + 1, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(obstacle.x + 3, obstacle.y, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x + 2, obstacle.y + 1, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x + 2, obstacle.y + 1, 0)), floor[2])){
                                 GenerateRangedEnemy(obstacle.x + 2, obstacle.y + 1);
                            }
                        }
                    }else if(spawnPosition == 1){ //LTOP
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x - 2, obstacle.y + 1, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(obstacle.x - 1, obstacle.y, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x - 2, obstacle.y + 1, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x - 2, obstacle.y + 1, 0)), floor[2])){
                                 GenerateRangedEnemy(obstacle.x - 2, obstacle.y + 1);
                            }
                        }
                    }else if(spawnPosition == 2){ //RBOT
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x + 2, obstacle.y - 3, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(obstacle.x + 3, obstacle.y - 4, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x + 2, obstacle.y - 3, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x + 2, obstacle.y - 3, 0)), floor[2])){
                                 GenerateRangedEnemy(obstacle.x + 2, obstacle.y - 3);
                            }
                        }
                    }else if(spawnPosition == 3){ //LBOT
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x - 2, obstacle.y - 3, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(obstacle.x - 1, obstacle.y - 4, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x - 2, obstacle.y - 3, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(obstacle.x - 2, obstacle.y - 3, 0)), floor[2])){
                                 GenerateRangedEnemy(obstacle.x - 2, obstacle.y - 3);
                            }

                        }
                    }
                }

                //USE MELEE ENEMY SPAWNS TO GENERATE RANGED ENEMIES
                foreach(Vector3Int enemy in meleeEnemySpawns){
                    int rangedEnemySpawnChance = UnityEngine.Random.Range(0, 10); // 1/10 spawn chance
                    int spawnPosition = UnityEngine.Random.Range(0, 4); // 0 TOP, 1 BOT, 2 RIGHT, 3 LEFT

                    if(spawnPosition == 0){ //TOP
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x, enemy.y + 2, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(enemy.x + 1, enemy.y + 1, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x, enemy.y + 2, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x, enemy.y + 2, 0)), floor[2])){
                                 GenerateRangedEnemy(enemy.x, enemy.y + 2);
                            }
                        }
                    }else if(spawnPosition == 1){ //BOTTOM
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x, enemy.y - 2, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(enemy.x + 1, enemy.y - 3, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x, enemy.y - 2, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x, enemy.y - 2, 0)), floor[2])){
                                 GenerateRangedEnemy(enemy.x, enemy.y - 2);
                            }
                        }
                    }else if(spawnPosition == 2){ //RIGHT
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x + 2, enemy.y, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(enemy.x + 3, enemy.y - 1, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x + 2, enemy.y, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x + 2, enemy.y, 0)), floor[2])){
                                 GenerateRangedEnemy(enemy.x + 2, enemy.y);
                            }
                        }
                    }else if(spawnPosition == 3){ //LEFT
                        if(rangedEnemySpawnChance == 0 && IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x - 2, enemy.y, 0)), 
                        (Tile)grid.GetTile(new Vector3Int(enemy.x - 1, enemy.y - 1, 0)))){

                            //Also check that spawning coords are a floor tile
                            if(IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x - 2, enemy.y, 0)), floor[0]) || 
                            IsSameTile((Tile)grid.GetTile(new Vector3Int(enemy.x - 2, enemy.y, 0)), floor[2])){
                                 GenerateRangedEnemy(enemy.x - 2, enemy.y);
                            }
                        }
                    }
                }
                
                //Room has been added so decrement rooms variable
                //Debug.Log("Room Generated!");
                roomsCount -= 1;
            }

        } //END WHILE LOOP

        //Mark the room that is farthest from the starting room as the exit room
        MarkEndRoom();
        
        //Debug.Log("Finished Generating All Rooms!");
        //Debug.Log(roomMidPoints.Count);
    }

    //Marks the room that is furthest from the starting room
    void MarkEndRoom()
    {
        Vector3Int furthestCoord = new Vector3Int();
        double furthestDistance = 0;

        foreach(Vector3Int coord in roomMidPoints){
            double distance = GetDistance(roomMidPoints[0].x, coord.x, roomMidPoints[0].y, coord.y);

            if(distance > furthestDistance){
                furthestCoord = coord;
                furthestDistance = distance;
            }
        }
        grid.SetTile(new Vector3Int(furthestCoord.x, furthestCoord.y, 0), floor[2]);
    }

    void GenerateHole(int xCoord, int yCoord)
    {
        //Generates top left to bottom right using 4x4 space 
        //Must check to see if selected tile has enough space to generate a hole

        //TOP ROW
        grid.SetTile(new Vector3Int(xCoord, yCoord, 0), floor[3]); 
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord, 0), floor[3]);

        //SECOND ROW
        grid.SetTile(new Vector3Int(xCoord - 1, yCoord - 1, 0), floor[3]);
        grid.SetTile(new Vector3Int(xCoord, yCoord - 1, 0), floor[3]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 1, 0), floor[3]);
        grid.SetTile(new Vector3Int(xCoord + 2, yCoord - 1, 0), floor[3]);

        //THRID ROW
        grid.SetTile(new Vector3Int(xCoord - 1, yCoord - 2, 0), floor[3]);
        grid.SetTile(new Vector3Int(xCoord, yCoord - 2, 0), floor[3]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 2, 0), floor[3]);
        grid.SetTile(new Vector3Int(xCoord + 2, yCoord - 2, 0), floor[3]);

        //BOTTOM ROW
        grid.SetTile(new Vector3Int(xCoord, yCoord - 3, 0), floor[3]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 3, 0), floor[3]);
    }

     void GenerateCover(int xCoord, int yCoord)
    {
        //Generates top left to bottom right using 4x4 space 
        //Must check to see if selected tile has enough space to generate a hole

        //TOP ROW
        grid.SetTile(new Vector3Int(xCoord, yCoord, 0), floor[1]); 
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord, 0), floor[1]);

        //SECOND ROW
        grid.SetTile(new Vector3Int(xCoord - 1, yCoord - 1, 0), floor[1]);
        grid.SetTile(new Vector3Int(xCoord, yCoord - 1, 0), floor[1]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 1, 0), floor[1]);
        grid.SetTile(new Vector3Int(xCoord + 2, yCoord - 1, 0), floor[1]);

        //THRID ROW
        grid.SetTile(new Vector3Int(xCoord - 1, yCoord - 2, 0), floor[1]);
        grid.SetTile(new Vector3Int(xCoord, yCoord - 2, 0), floor[1]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 2, 0), floor[1]);
        grid.SetTile(new Vector3Int(xCoord + 2, yCoord - 2, 0), floor[1]);

        //BOTTOM ROW
        grid.SetTile(new Vector3Int(xCoord, yCoord - 3, 0), floor[1]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 3, 0), floor[1]);
    }

    void GenerateMeleeEnemy(int xCoord, int yCoord)
    {
        //Generates top left to bottom right using 2x2 space
        //Must check to see if selected tile has enough space to generate an enemy

        grid.SetTile(new Vector3Int(xCoord, yCoord, 0), enemies[0]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord, 0), enemies[0]);
        grid.SetTile(new Vector3Int(xCoord, yCoord - 1, 0), enemies[0]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 1, 0), enemies[0]);
    }

    void GenerateRangedEnemy(int xCoord, int yCoord)
    {
        //Generates top left to bottom right using 2x2 space
        //Must check to see if selected tile has enough space to generate an enemy

        grid.SetTile(new Vector3Int(xCoord, yCoord, 0), enemies[1]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord, 0), enemies[1]);
        grid.SetTile(new Vector3Int(xCoord, yCoord - 1, 0), enemies[1]);
        grid.SetTile(new Vector3Int(xCoord + 1, yCoord - 1, 0), enemies[1]);
    }

    private double GetDistance(double x1, double x2, double y1, double y2)
    {
        return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    //Use (Tile)grid.GetTile(new vector3Int(x,y,z)) AND floor[x] for hyperparameters
    private bool IsSameTile(Tile tile1, Tile tile2) 
    {
        bool check;
        if(tile1 == tile2){
             check = true;
        }else{
             check = false;
        }

        return check;
    }

    void GenerateRoomConnections()
    {
        //Select Starting Room roomMidPoint to begin and remove it from list
        Vector3Int currentCoord = roomMidPoints[0];
        roomMidPoints.Remove(currentCoord);

        //Start loop here
        while(roomMidPoints.Count != 0){

            //Iterate and choose the point closest to current point 
            double closestDistance = 0;
            Vector3Int closestCoord = new Vector3Int();
            foreach(Vector3Int coord in roomMidPoints){
                double distance = GetDistance(currentCoord.x, coord.x, currentCoord.y, coord.y);

                if(closestDistance == 0){
                    closestDistance = distance;
                    closestCoord = coord;
                }else{
                    if(distance < closestDistance){
                        closestDistance = distance;
                        closestCoord = coord;
                    }
                }
            }

            //Build a path from currentCoord to closestCoord by matching coords
            if(currentCoord.x < closestCoord.x){ //Do a for loop to increment
                int newX;
                for(newX = 0; newX + currentCoord.x < closestCoord.x; newX++){ //RIGHT
                    if(!grid.HasTile(new Vector3Int(newX + currentCoord.x, currentCoord.y, 0))){
                        grid.SetTile(new Vector3Int(newX + currentCoord.x, currentCoord.y, 0), floor[0]);
                        grid.SetTile(new Vector3Int(newX + currentCoord.x, currentCoord.y + 1, 0), floor[0]);
                        grid.SetTile(new Vector3Int(newX + currentCoord.x, currentCoord.y - 1, 0), floor[0]);
                    }
                }
                currentCoord.x += newX;
            }else if(currentCoord.x > closestCoord.x){ //Do a for loop to decrement
                int newX;
                for(newX = 0; currentCoord.x - newX > closestCoord.x; newX++){ //LEFT
                    if(!grid.HasTile(new Vector3Int(currentCoord.x - newX, currentCoord.y, 0))){
                        grid.SetTile(new Vector3Int(currentCoord.x - newX, currentCoord.y, 0), floor[0]);
                        grid.SetTile(new Vector3Int(currentCoord.x - newX, currentCoord.y + 1, 0), floor[0]);
                        grid.SetTile(new Vector3Int(currentCoord.x - newX, currentCoord.y - 1, 0), floor[0]);
                    }
                }
                currentCoord.x -= newX;
            }

            //Move on to Y coords now that x coords have been matched
            if(currentCoord.y < closestCoord.y){ //Do a for loop to increment
                int meleeEnemySpawnChance = UnityEngine.Random.Range(0, 4); // 1/4 spawn chance in corridors

                if(meleeEnemySpawnChance == 0){
                    GenerateMeleeEnemy(currentCoord.x, currentCoord.y + 2);
                }

                for(int y = 0; y + currentCoord.y < closestCoord.y; y++){ //UP
                    if(!grid.HasTile(new Vector3Int(currentCoord.x, currentCoord.y + y, 0))){
                        grid.SetTile(new Vector3Int(currentCoord.x, currentCoord.y + y, 0), floor[0]);
                        grid.SetTile(new Vector3Int(currentCoord.x + 1, currentCoord.y + y, 0), floor[0]);
                        grid.SetTile(new Vector3Int(currentCoord.x - 1, currentCoord.y + y, 0), floor[0]);
                    }
                }
            }else if(currentCoord.y > closestCoord.y){ //Do a for loop to decrement
                int meleeEnemySpawnChance = UnityEngine.Random.Range(0, 4); // 1/4 spawn chance in corridors

                if(meleeEnemySpawnChance == 0){
                    GenerateMeleeEnemy(currentCoord.x, currentCoord.y - 2);
                }
                
                for(int y = 0; currentCoord.y - y > closestCoord.y; y++){ //DOWN
                    if(!grid.HasTile(new Vector3Int(currentCoord.x, currentCoord.y - y, 0))){
                        grid.SetTile(new Vector3Int(currentCoord.x, currentCoord.y - y, 0), floor[0]);
                        grid.SetTile(new Vector3Int(currentCoord.x + 1, currentCoord.y - y, 0), floor[0]);
                        grid.SetTile(new Vector3Int(currentCoord.x - 1, currentCoord.y - y, 0), floor[0]);
                    }
                }
            }

            //Update the values of currentCoord and closestCoord for next connection
            currentCoord = closestCoord;
            roomMidPoints.Remove(currentCoord);
        } //END WHILE LOOP

    } //END OF GenerateRoomConnections

    /*
    GeneratePath is not utilized in the final project's build but is left in the code because the final
    paper talks about it in further detail.
    */
    void GeneratePath()
    {
        //choose random y starting value within grid dimensions
        int yValStart = UnityEngine.Random.Range(0, yDimensionSize);
        //Debug.Log("y Value start = " + yValStart.ToString());
        int rand;

        //place starter tile
        grid.SetTile(new Vector3Int(0, yValStart, 0), floor[2]);

        //Decision loop to select UP, DOWN, OR RIGHT direction for tile
        for(int x = 0; x<xDimensionSize - 1;){

            if(yValStart == yDimensionSize - 1){ //TOP BOUNDARY
                rand = UnityEngine.Random.Range(1, 3); //Add to x or sub from y
            }else if(yValStart == 0){ //BOTTOM BOUNDARY
                rand = UnityEngine.Random.Range(0, 2); //Add to x or add to y
            }else{ //ANYTHING IN BETWEEN
                rand = UnityEngine.Random.Range(0, 3); //Any of the three options
            }
            
            if(rand == 0){ //ADD TO Y
                if(!grid.HasTile(new Vector3Int(x, yValStart + 1, 0))){
                        grid.SetTile(new Vector3Int(x, yValStart + 1, 0), floor[2]);
                        yValStart++;
                        //Debug.Log("Rand = 0 UP");
                        //Debug.Log("y Value start = " + yValStart.ToString());
                    }
            }else if(rand == 1){ //ADD TO X
                grid.SetTile(new Vector3Int(x + 1, yValStart, 0), floor[2]);
                x++;  
                //Debug.Log("Rand = 1 RIGHT");
                //Debug.Log("y Value start = " + yValStart.ToString());
            }else if(rand == 2){ //SUB FROM Y
                if(!grid.HasTile(new Vector3Int(x, yValStart - 1, 0))){
                        grid.SetTile(new Vector3Int(x, yValStart - 1, 0), floor[2]);
                        yValStart--;
                        //Debug.Log("Rand = 2 DOWN");
                        //Debug.Log("y Value start = " + yValStart.ToString());
                    }
            }
        }
    }    

}
