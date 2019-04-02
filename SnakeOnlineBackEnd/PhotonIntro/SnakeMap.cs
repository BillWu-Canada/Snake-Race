using ExitGames.Logging;
using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// new version 2.5.1 here !! welcome everyone to test.

namespace PhotonIntro
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using System;
    using System.Timers;
    using System.Collections;
    using System.Collections.Generic;

    class SnakeMap
    {
        public class Node
        {
            public int row { get; set; }
            public int col { get; set; }

            public Node(int x, int y)
            {
                this.row = x;
                this.col = y;
            }

            public bool is_equal(Node other)
            {
                if (other == null)
                {
                    return false;
                }
                return this.row == other.row && this.col == other.col;
            }
        }

        public enum COLOR
        {
            EMPTY,     //0
            P1,        //1
            P2,        //2
            P3,        //3
            P4,        //4
            FOOD       //5
        }

        public enum EVENT
        {
            NONE,        //0
            NORMAL,      //1
            ATE,         //2
            CUT,         //3
            BECUT,       //4
            KILLED,      //5
            WIN          //6
        }

        private static int grid_size = 15;
        private bool initialized = false;
        private List<List<Node>> players_list = new List<List<Node>>();
        private List<Node> fruit_list = new List<Node>();
        private List<Node> fruit_buffer = new List<Node>();

        private List<Node> ghost_head = new List<Node>();

        // the original direction of the head for each snakes
        private int[] head_direction = new int[4] { -1, -1, -1, -1 };
        // list store events
        private string[] events = new string[4] { "None", "None", "None", "None" };

        private int[] dead_time = new int[4] { 0, 0, 0, 0 };


        private int[] map_returned = new int[225];
        private int[] events_returned = new int[4];


        private int current_time_in_seconds()
        {
            DateTime current_time = DateTime.Now;

            int hours = current_time.Hour;
            int minutes = current_time.Minute;
            int seconds = current_time.Second;

            return ((hours * 60) + minutes) * 60 + seconds;
        }

        private void set_dead_time(int i)
        {
            int time = current_time_in_seconds();

            dead_time[i] = time;
        }

        public void fill_map_returned()
        {
            // flush map
            for (int i = 0; i < 225; i++)
            {
                map_returned[i] = 0;
            }

            //scan player_list into the map
            int playerID = 1;

            foreach (var sublist in players_list)
            {
                foreach (var value in sublist)
                {
                    int row = value.row;
                    int col = value.col;
                    int index = row * grid_size + col;

                    if (map_returned[index] == 0)
                    {
                        map_returned[index] = playerID;
                    }
                    else
                    {
                        Console.WriteLine("overwrite happend when player_list filling the map !!");
                        Console.WriteLine("overwrite position: " + row + "," + col);
                        Console.WriteLine("original map_returned value there: " + map_returned[index]);
                        Environment.Exit(1);
                    }
                }
                playerID += 1;
            }

            //scan fruit list into the map
            foreach (var value in fruit_list)
            {
                int row = value.row;
                int col = value.col;
                int index = row * grid_size + col;

                if (map_returned[index] == 0)
                {
                    map_returned[index] = 5;
                }
                else
                {
                    Console.WriteLine("overwrite happend when fruit_list filling the map !!");
                    Console.WriteLine("overwrite position: " + row + "," + col);
                    Console.WriteLine("original map_returned value there: " + map_returned[index]);
                    Environment.Exit(1);
                }

            }
        }

        public void initialize_map(int num_of_players)
        {
            // initialize each player
            for (int i = 0; i < num_of_players; i++)
            {
                List<Node> each_player = new List<Node>();

                if (i == 0)
                {
                    each_player.Add(new Node(2, 0));
                    each_player.Add(new Node(1, 0));
                    each_player.Add(new Node(0, 0));
                    // head direction (down)
                    head_direction[0] = 2;
                    // event
                    events[0] = "Normal";
                }
                else if (i == 1)
                {
                    each_player.Add(new Node(12, 14));
                    each_player.Add(new Node(13, 14));
                    each_player.Add(new Node(14, 14));
                    // head direction (up)
                    head_direction[1] = 0;
                    // event
                    events[1] = "Normal";
                }
                else if (i == 2)
                {
                    each_player.Add(new Node(0, 12));
                    each_player.Add(new Node(0, 13));
                    each_player.Add(new Node(0, 14));
                    // head direction (left)
                    head_direction[2] = 3;
                    // event
                    events[2] = "Normal";
                }
                else if (i == 3)
                {
                    each_player.Add(new Node(14, 2));
                    each_player.Add(new Node(14, 1));
                    each_player.Add(new Node(14, 0));
                    // head direction (right)
                    head_direction[3] = 1;
                    // event
                    events[3] = "Normal";
                }
                else
                {
                    Console.WriteLine("number of player is NOT correct !!");
                    Environment.Exit(1);
                }

                players_list.Add(each_player);

                ghost_head.Add(new Node(-1, -1));
            }

            // add fruit
            fruit_list.Add(new Node(7, 7));

            initialized = true;
        }


        int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public void snake_to_food_for_lost_connection(int i)
        {
            foreach (var value in players_list[i])
            {
                int row = value.row;
                int col = value.col;

                fruit_list.Add(new Node(row, col));
            }
        }

        public void push_to_fruit_buffer(int x, int y)
        {
            Node input = new Node(x, y);

            foreach (var value in fruit_buffer)
            {
                if (input.is_equal(value))
                {
                    // find duplicate
                    return;
                }
            }

            // add to buffer
            fruit_buffer.Add(input);
        }

        public void fruit_buffer_remove_fruit_list()
        {
            foreach (var value in fruit_buffer)
            {
                foreach (var fruit in fruit_list)
                {
                    if (value.is_equal(fruit))
                    {
                        fruit_list.Remove(fruit);
                        break;
                    }
                }
            }

            // clear fruit buffer
            fruit_buffer.Clear();
        }

        private void snake_move(int i, int new_direction)
        {
            int num_of_snake_body = players_list[i].Count;

            if (num_of_snake_body == 0)
            {
                if (new_direction == 4)
                {
                    set_dead_time(i);
                }
                return;
            }

            // directions are 0:up ; 1:right ; 2:down ; 3:left

            // base case for directions -2:disconnected, -1:didn't touch the screen
            else if (new_direction == -2)
            {
                events[i] = "Killed";
                // turn the snake into the food
                snake_to_food_for_lost_connection(i);
                players_list[i].RemoveRange(0, players_list[i].Count);
                dead_time[i] = -111;

                return;
            }

            // if new_direction is exactly opposite to old head direction
            else if (new_direction == -1 || Math.Abs(head_direction[i] - new_direction) == 2)
            {
                // maintain the old direction , use head_direction[i]
                //Log.Debug("maintain the same direction");
            }

            else
            {
                head_direction[i] = new_direction;
            }

            // update snake body in player_list

            int new_head_row = players_list[i][0].row;
            int new_head_col = players_list[i][0].col;

            ghost_head[i].row = new_head_row;
            ghost_head[i].col = new_head_col;

            // direction is up
            if (head_direction[i] == 0)
            {
                new_head_row = mod((players_list[i][0].row - 1), grid_size);
            }
            // direction is right
            else if (head_direction[i] == 1)
            {
                new_head_col = mod((players_list[i][0].col + 1), grid_size);
            }
            // direction is down
            else if (head_direction[i] == 2)
            {
                new_head_row = mod((players_list[i][0].row + 1), grid_size);
            }
            //  direction is left
            else if (head_direction[i] == 3)
            {
                new_head_col = mod((players_list[i][0].col - 1), grid_size);
            }
            // recon
            else if (head_direction[i] == 4)
            {
                set_dead_time(i);
            }
            else
            {
                Console.WriteLine("head_direction has some error in function head_update");
                Environment.Exit(1);
            }

            // add new head
            players_list[i].Insert(0, new Node(new_head_row, new_head_col));

            Tuple<int, int> checker = null;

            checker = head_to_fruit_detect(i);

            if (checker == null)
            {
                // pop the tail
                int tail_index = players_list[i].Count - 1;
                players_list[i].RemoveAt(tail_index);
            }

            if (checker != null)
            {
                // keep the tail and remove the fruit from the list
                Console.WriteLine("player" + (i + 1) + ": " + "eat fruit at: " + checker.Item1 + " " + checker.Item2);
                events[i] = "Ate";

                //delete_fruit(checker.Item1, checker.Item2);
                push_to_fruit_buffer(checker.Item1, checker.Item2);

            }

        }

        //----------------------------------------------------------------------------------------------    
        public List<Node> head_to_head_detect(int i)
        {
            Console.WriteLine("player" + (i + 1) + " get in head_to_head_detect()");

            List<Node> returned_Node = new List<Node>();

            int total_players = players_list.Count;

            for (int j = 0; j < total_players; j++)
            {
                if (players_list[j].Count > 0 && j != i && players_list[j][0].is_equal(players_list[i][0]))
                {
                    Console.WriteLine("j: " + j + " i: " + i);


                    returned_Node.Add(new Node(j, 0));
                }
            }

            return returned_Node;
        }


        // delete snake body after start_pos and pop start_pos at snake_index
        public void turn_snake_into_food(int snake_index, int start_pos)
        {
            Console.WriteLine("player" + (snake_index + 1) + " get into turn_snake_into_food");
            for (int i = players_list[snake_index].Count - 1; i > start_pos; i--)
            {
                //Console.WriteLine("i: " + i);
                // put body into fruit list
                int row = players_list[snake_index][i].row;
                int col = players_list[snake_index][i].col;

                fruit_list.Add(new Node(row, col));
            }

            // pop snake body
            int body_count = players_list[snake_index].Count;

            for (int i = body_count - 1; i >= start_pos; i--)
            {
                players_list[snake_index].RemoveAt(i);
            }

        }


        public void head_collision_handle(int i, List<Node> head_checker)
        {

            head_checker.Add(new Node(i, 0));
            int max_snake_count = 0;

            int current_max_body_length = players_list[head_checker[0].row].Count;

            for (int k = 1; k < head_checker.Count; k++)
            {
                int snake_body_length = players_list[head_checker[k].row].Count;

                if (snake_body_length >= current_max_body_length)
                {
                    current_max_body_length = snake_body_length;
                }
            }

            for (int k = 0; k < head_checker.Count; k++)
            {
                int snake_body_length = players_list[head_checker[k].row].Count;

                if (current_max_body_length == snake_body_length)
                {
                    max_snake_count++;
                }
            }

            if (max_snake_count > 1)
            {
                // at least two snakes have maximun body, all snake die
                for (int k = 0; k < head_checker.Count; k++)
                {
                    int snake_index = head_checker[k].row;
                    turn_snake_into_food(snake_index, 0);
                    events[snake_index] = "Killed";
                    set_dead_time(snake_index);
                }
            }

            else if (max_snake_count == 1)
            {
                // the snake has longest body live, others die
                for (int k = 0; k < head_checker.Count; k++)
                {
                    int snake_index = head_checker[k].row;
                    int body_length = players_list[snake_index].Count;

                    if (body_length != current_max_body_length)
                    {
                        // smaller snake, kill it
                        turn_snake_into_food(snake_index, 0);
                        events[snake_index] = "Killed";
                        set_dead_time(snake_index);
                    }
                }
            }

            else
            {
                Console.WriteLine("head_colloision_handle error, max_snake_count can't be 0 !!");
                Environment.Exit(1);
            }

        }


        public Tuple<int, int> head_to_body_detect(int i)
        {

            for (int x = 0; x < players_list.Count; x++)
            {
                for (int y = 1; y < players_list[x].Count; y++)
                {

                    if (players_list[x][y].is_equal(players_list[i][0]))
                    {
                        // find the postion of body cut
                        events[x] = "Becut";
                        Tuple<int, int> T = new Tuple<int, int>(x, y);
                        return T;
                    }

                }
            }

            return null;
        }


        public Tuple<int, int> head_to_fruit_detect(int i)
        {
            foreach (var value in fruit_list)
            {
                if (players_list[i][0].is_equal(value))
                {
                    Tuple<int, int> T = new Tuple<int, int>(value.row, value.col);
                    return T;
                }
            }

            return null;
        }


        public void delete_fruit(int fruit_pos_x, int fruit_pos_y)
        {
            // remove fruit from fruit list
            for (int i = 0; i < fruit_list.Count; i++)
            {
                if (fruit_list[i].row == fruit_pos_x && fruit_list[i].col == fruit_pos_y)
                {
                    // find the fruit we want to delete
                    fruit_list.RemoveAt(i);
                    break;
                }
            }

        }


        public int head_to_head_hard_check(int i)
        {
            for (int k = 0; k < players_list.Count; k++)
            {
                if (k != i && players_list[k].Count > 0)
                {
                    if (players_list[i][0].is_equal(ghost_head[k]) && players_list[k][0].is_equal(ghost_head[i]))
                    {
                        // find the collision
                        return k;
                    }
                }
            }

            return -1;
        }

        // search body within snake k
        public bool search_body(Node nd, int k)
        {
            foreach (var body in players_list[k])
            {
                if (nd.is_equal(body))
                {
                    return true;
                }
            }

            return false;
        }

        // put snake a's body (not overlap with snake b) into fruit list
        public void put_body_into_fruit_list(int a, int b)
        {
            foreach (var value in players_list[a])
            {
                if (search_body(value, b) == false)
                {
                    fruit_list.Add(new Node(value.row, value.col));
                }
            }
        }

        public void hard_head_collision_handle(int i, int k)
        {
            int snake_i_length = players_list[i].Count;
            int snake_k_length = players_list[k].Count;

            if (snake_i_length < snake_k_length)
            {
                // snake i die
                put_body_into_fruit_list(i, k);
                players_list[i].Clear();
                events[i] = "Killed";
                set_dead_time(i);
            }

            else if (snake_i_length > snake_k_length)
            {
                // snake k die
                put_body_into_fruit_list(k, i);
                players_list[k].Clear();
                events[k] = "Killed";
                set_dead_time(k);
            }

            else
            {
                // both die
                put_body_into_fruit_list(i, k);
                players_list[i].Clear();
                events[i] = "Killed";
                set_dead_time(i);

                put_body_into_fruit_list(k, i);
                players_list[k].Clear();
                events[k] = "Killed";
                set_dead_time(k);
            }
        }

        public int fruit_pos_check(int i)
        {
            for (int k = 0; k < fruit_list.Count; k++)
            {
                if (fruit_list[k].is_equal(players_list[i][0]))
                {
                    return k;
                }
            }

            return -1;
        }

        public void competitive_mode_update(int i)
        {
            // 0. check whether this snake is dead or live
            // 1. check head to head collision
            // 2. check head to body eat (including itself)
            // 3. check head to fruit eat (apply it in snake_move)
            // 4. check normal move (don't apply any thing on it)
            // 5. check head to head (hard)


            // check case 0
            if (players_list[i].Count == 0)
            {
                events[i] = "Killed";
                return;
            }


            int collision_oppos = -1;

            // check case 5
            collision_oppos = head_to_head_hard_check(i);

            if (collision_oppos != -1)
            {
                hard_head_collision_handle(i, collision_oppos);
                return;
            }


            Tuple<int, int> checker = null;

            // check case 2
            checker = head_to_body_detect(i);
            if (checker != null)
            {
                turn_snake_into_food(checker.Item1, checker.Item2);
                events[i] = "Cut";
                return;
            }


            List<Node> head_checker = null;
            head_checker = head_to_head_detect(i);

            // check case 1 (updated)
            if (head_checker.Count > 0)
            {
                // head to head collision happen!!
                head_collision_handle(i, head_checker);
                return;
            }

            int fruit_pos = -1;
            fruit_pos = fruit_pos_check(i);

            if (fruit_pos != -1)
            {
                fruit_list.RemoveAt(fruit_pos);
                return;
            }


        }


        // test code: print
        public void print_player_list(List<List<Node>> list)
        {
            Console.WriteLine("all players are:");
            foreach (var sublist in list)
            {
                foreach (var value in sublist)
                {
                    Console.Write(value.row);
                    Console.Write(',');
                    Console.Write(value.col);
                    Console.Write(" ; ");
                }
                Console.WriteLine();
            }
        }

        public void print_fruit_list(List<Node> list)
        {
            Console.WriteLine("all fruits are:");
            foreach (var value in list)
            {
                Console.Write(value.row);
                Console.Write(',');
                Console.Write(value.col);
                Console.Write(" ; ");
            }
            Console.WriteLine();
        }

        public void print_events(string[] list)
        {
            Console.Write("events are: ");
            foreach (var value in list)
            {
                Console.Write(value);
                Console.Write(" , ");
            }
            Console.WriteLine();
        }

        public void print_map(int[] map)
        {
            int k = 0;

            for (int i = 0; i < grid_size; i++)
            {
                for (int j = 0; j < grid_size; j++)
                {
                    if (map[k] == 0)
                    {
                        Console.Write('.');
                    }
                    else
                    {
                        Console.Write(map[k]);
                    }

                    Console.Write(' ');
                    k++;
                }
                Console.WriteLine();
            }
        }

        void fruit_genrator(int timer_counter)
        {
            Random rand = new Random();

            if ((timer_counter % 15) == 0)
            {
                while (true)
                {
                    int row = rand.Next(0, 15);
                    int col = rand.Next(0, 15);

                    int index = row * grid_size + col;

                    if (map_returned[index] == 0)
                    {
                        // legal to put a fruit on the map
                        fruit_list.Add(new Node(row, col));
                        map_returned[index] = 5;

                        break;
                    }
                }
            }
        }



        private Dictionary<byte, object> update_returned_packet()
        {
            for (int i = 0; i < events.Length; i++)
            {
                string e = events[i];

                if (e.Equals("None"))
                {
                    events_returned[i] = 0;
                }
                else if (e.Equals("Normal"))
                {
                    events_returned[i] = 1;
                }
                else if (e.Equals("Ate"))
                {
                    events_returned[i] = 2;
                }
                else if (e.Equals("Cut"))
                {
                    events_returned[i] = 3;
                }
                else if (e.Equals("Becut"))
                {
                    events_returned[i] = 4;
                }
                else if (e.Equals("Killed"))
                {
                    events_returned[i] = 5;
                }
                else if (e.Equals("Win"))
                {
                    events_returned[i] = 6;
                }
                else
                {
                    //Log.Error("event not matched...");
                    Console.WriteLine("event not matched: " + e);
                }
            }

            Dictionary<byte, object> dict = new Dictionary<byte, object>();

            dict.Add(0, (object)map_returned);
            dict.Add(1, (object)events_returned);

            return dict;
        }


        public void revive_snake(int i)
        {
            Random rand = new Random();

            for (int k = 0; k < 15; k++)
            {
                int revive_row = rand.Next(0, grid_size);
                int revive_col_1 = rand.Next(0, grid_size - 2);
                int revive_col_2 = revive_col_1 + 1;
                int revive_col_3 = revive_col_1 + 2;

                int r_pos_1 = revive_row * grid_size + revive_col_1;
                int r_pos_2 = revive_row * grid_size + revive_col_2;
                int r_pos_3 = revive_row * grid_size + revive_col_3;

                if (map_returned[r_pos_1] == 0 && map_returned[r_pos_2] == 0 && map_returned[r_pos_3] == 0)
                {
                    players_list[i].Add(new Node(revive_row, revive_col_1));
                    players_list[i].Add(new Node(revive_row, revive_col_2));
                    players_list[i].Add(new Node(revive_row, revive_col_3));

                    map_returned[r_pos_1] = i + 1;
                    map_returned[r_pos_2] = i + 1;
                    map_returned[r_pos_3] = i + 1;

                    head_direction[i] = 3;

                    events[i] = "Normal";

                    break;
                }

            }


        }


        int timer_counter = 0;

        public Dictionary<byte, object> Map_Update(List<int> packet)
        {
            timer_counter += 1;

            int num_of_players = packet.Count;

            if (initialized == false)
            {
                initialize_map(num_of_players);
            }

            for (int i = 0; i < events.Length; i++)
            {
                events[i] = "Normal";
            }

            for (int i = 0; i < num_of_players; i++)
            {
                // update every snake's head position regardless
                snake_move(i, packet[i]);
            }

            fruit_buffer_remove_fruit_list();

            for (int i = 0; i < num_of_players; i++)
            {
                // update snake postion and food position on board
                competitive_mode_update(i);
            }

            // check winner
            for (int i = 0; i < num_of_players; i++)
            {
                int body_length = players_list[i].Count;
                if (body_length >= 15)
                {
                    events[i] = "Win";
                }
            }



            // fill the map at the end
            fill_map_returned();

            // revive the dead snake after 5 seconds
            for (int i = 0; i < num_of_players; i++)
            {
                if (players_list[i].Count == 0 && dead_time[i] != -111)
                {
                    int current_time = current_time_in_seconds();
                    int time_interval = current_time - dead_time[i];

                    if (time_interval < 0)
                    {
                        time_interval += 24 * 60 * 60;
                    }

                    if (time_interval >= 5)
                    {
                        // revive it
                        revive_snake(i);
                    }
                }
            }


            // random generate fruit
            fruit_genrator(timer_counter);

            Console.WriteLine("map after updated: ");
            print_map(map_returned);
            Console.WriteLine();
            print_events(events);
            Console.WriteLine();

            // packet all info to send to the controller
            Dictionary<byte, object> dict = update_returned_packet();
            return dict;
        }

        /*
        static void Main(string[] args)
        {
            Console.WriteLine("Bill: hello world haha !!");
            Console.WriteLine();


            SnakeMap SM = new SnakeMap();
            SM.print_player_list(SM.players_list);



            Console.WriteLine();

            List<int> packet = new List<int> { -1, -1, -1, -1 };

            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);

            packet = new List<int> { 1, 3, -1, -1 };

            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);     // snake2 head to head with snake1 here

            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);

            System.Threading.Thread.Sleep(5000);

            packet = new List<int> { 2, 2, 2, 2 };

            SM.Map_Update(packet);
            SM.Map_Update(packet);
            SM.Map_Update(packet);

        }
        */
    }



}


