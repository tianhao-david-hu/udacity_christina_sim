﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
	[RequireComponent (typeof(CarController))]
	public class CarTraffic : MonoBehaviour {

	//forward cars
	[SerializeField] private List<GameObject> cars;
        //private List<double> cars_previous_vx;
        //private List<double> cars_previous_vy;
    double[] cars_previous_vx;
    double[] cars_previous_vy;
        int data_count;
        bool b_previous_v_initialized;

    private Queue<GameObject> inactive_cars;

	public GameObject maincar;

	//reverse cars
	[SerializeField] private List<GameObject> carsR;
	private Queue<GameObject> inactive_carsR;
	
	//use counter to update every second
	private int counter;
	//number of cars to push at update, max is 3
	private bool init;
	private int count_max;

	// Use this for initialization
	void Start () {

		inactive_cars = new Queue<GameObject>();
		inactive_carsR = new Queue<GameObject>();
		counter = 0;
		// push two cars at a time

		init = true;
		count_max = Random.Range (20, 60);


            cars_previous_vx = new double[12];
            cars_previous_vy = new double[12];
            data_count=0;
            b_previous_v_initialized=false;

        }
			
	// Update is called once per frame
	void Update () {
		if (counter >= count_max || init) {
			init = false;
			UpdateForward ();
			UpdateReverse ();
			counter = 0;
			count_max = Random.Range (20, 60);
		}
		else 
		{
			counter++;
		}
	}
	void FixedUpdate()
	{
			foreach (GameObject car in cars) {
				CarAIControl carAI = (CarAIControl)car.GetComponent (typeof(CarAIControl));
				List<float> frenet_values = carAI.getThisFrenetFrame ();
			}
	}

	
	public void UpdateForward(){
		//add any inactive car to inactive car list
		foreach (GameObject car in cars) 
		{
			CarAIControl carAI = (CarAIControl) car.GetComponent(typeof(CarAIControl));
			if (carAI.RegenerateCheck ()) {
				//turn off the car and add it to a list
				carAI.setStage();
				inactive_cars.Enqueue (car);
			} 

		}
		// count how many cars we have pushed
		int push = Random.Range(1,4);
		int pushed = 0;
		
		while ((inactive_cars.Count > 0) && (pushed < push)) {
			GameObject inactive_car = inactive_cars.Dequeue();

			CarAIControl carAI = (CarAIControl) inactive_car.GetComponent(typeof(CarAIControl));
		
			carAI.Spawn (cars);

			pushed++;
		}
		
	}
	public void UpdateReverse(){
		//add any inactive car to inactive car list
		foreach (GameObject car in carsR) 
		{
			CarAIControl carAI = (CarAIControl) car.GetComponent(typeof(CarAIControl));
			if (carAI.RegenerateCheck ()) {
				//turn off the car and add it to a list
				carAI.setStage();
				inactive_carsR.Enqueue (car);
			} 

		}
		// count how many cars we have pushed
		int push = Random.Range(1,4);
		int pushed = 0;
		
		while ((inactive_carsR.Count > 0) && (pushed < push)) {
				GameObject inactive_car = inactive_carsR.Dequeue();

				CarAIControl carAI = (CarAIControl) inactive_car.GetComponent(typeof(CarAIControl));

				carAI.Spawn (carsR);

				pushed++;
		}
	}

	public bool lane_clear(GameObject mycar,bool forward,int lane)
	{
			List<float> s_values = new List<float> ();
			List<int> index_values = new List<int> ();
			int s_index = -1;

			int index = 1;

			List<float> d_array = new List<float> ();

			if (forward) 
			{
				
				foreach(GameObject car in cars) 
				{
					CarAIControl carAI = (CarAIControl)car.GetComponent (typeof(CarAIControl));

					//List<float> frenet_values = carAI.getThisFrenetFrame ();

					d_array.Add (carAI.getD ());

					if (mycar.GetInstanceID () == car.GetInstanceID ()) 
					{
						s_index = s_values.Count;
						s_values.Add (carAI.getS());
						index_values.Add (index);
					}

					else 
					{
						float d_value = carAI.getD ();//frenet_values [1];
						if( ((d_value < (2+lane*4+2)) && (d_value > (2+lane*4-2) )) )// || carAI.BlinkerLight())
						{
							s_values.Add (carAI.getS());
							index_values.Add (index);
						}

					}

					index++;
						

				}

				CarAIControl carAImain = (CarAIControl)maincar.GetComponent (typeof(CarAIControl));

				//List<float> frenet_values_main = carAImain.getThisFrenetFrame ();
				float d_value_main = carAImain.getD();//frenet_values_main[1];
				//check if main car is in lane
				if((d_value_main < (2+lane*4+3)) && (d_value_main > (2+lane*4-3)))
				{
					s_values.Add (carAImain.getS());
					index_values.Add (-1);
				}

			} 
			else 
			{
				
				foreach (GameObject car in carsR) 
				{
					CarAIControl carAI = (CarAIControl)car.GetComponent (typeof(CarAIControl));

					List<float> frenet_values = carAI.getThisFrenetFrame ();

					if (mycar.GetInstanceID () == car.GetInstanceID ()) 
					{
						s_index = s_values.Count;
						s_values.Add (frenet_values [0]);
					} 
					else 
					{
						float d_value = frenet_values [1];
						if( ((d_value < (2+lane*4+2)) && (d_value > (2+lane*4-2))) || carAI.BlinkerLight())
						{
							s_values.Add (frenet_values[0]);
						}
					}
						
				}
					
			}

			bool clear = true;
			//if (forward) 
			//{
			//	Debug.Log ("I am car " + index_values [s_index] + " change into lane "+lane);
			//}
				

			for (int i = 0; i < s_values.Count; i++) 
			{
				
				if (i != s_index) 
				{
					
					clear = clear && (((s_values [i] - s_values [s_index]) > 20) || ((s_values [s_index] - s_values [i]) > 20));
					//if (forward) 
					//{
					//	Debug.Log ("car " + index_values [i] + " is this far " + (s_values [i] - s_values [s_index]));
					//}
				}
			}

			/*
			if (forward) 
			{
				if (clear) 
				{
					Debug.Log ("Its safe");
				} 
				else
				{
					Debug.Log ("Not safe");
				}
				for (int i = 0; i < d_array.Count; i++) 
				{
					int d_elem = i + 1;
					Debug.Log ("d" + d_elem + " " + d_array [i]);
				}
			}
			*/
				
			return clear;

	}

	public string example_sensor_fusion()
	{
			string result = "[";
			int car_id = 0;

            for (int i = 0; i < cars.Count; i++)
            {
                CarAIControl carAI = (CarAIControl)cars[i].GetComponent(typeof(CarAIControl));

                //List<float> test_values = carAI.getFrenetFrame (1682.316f,2968.043f);

                //Debug.Log ("test values "+test_values [0] + "," + test_values [1]);

                if (car_id > 0)
                {
                    result += ",";
                }

                List<float> frenet_values = carAI.getThisFrenetFrame();

                if (System.Single.IsNaN(frenet_values[0]))
                {
                    frenet_values[0] = 0;
                }
                if (System.Single.IsNaN(frenet_values[1]))
                {
                    frenet_values[1] = 0;
                }


                //Debug.Log (car.transform.position.x+","+car.transform.position.z+","+frenet_values[0]+","+frenet_values[1]);

                double x= cars[i].transform.position.x;
                double y= cars[i].transform.position.z;
                double v_x = cars[i].GetComponent<Rigidbody>().velocity.x;
                double v_y = cars[i].GetComponent<Rigidbody>().velocity.z;

                double a_x=0, a_y=0, dt = 0.02F;
                double a_very_large_number = 4096;

                if(b_previous_v_initialized)
                {
                    a_x = (v_x - cars_previous_vx[i]) / dt;
                    a_y = (v_y - cars_previous_vy[i]) / dt;
                }
                else
                {
                    a_x = a_very_large_number;
                    a_y = a_very_large_number;
                }
                

                cars_previous_vx[i] = v_x;
                cars_previous_vy[i] = v_y;
                data_count++;
                if (data_count >= 12)
                    b_previous_v_initialized = true;


                 /*
                 if (cars_previous_vx.Count == cars.Count && cars_previous_vy.Count == cars.Count)
                 {
                     //a_x = (v_x - cars_previous_vx[i]) / dt;
                     //a_y = (v_y - cars_previous_vy[i]) / dt;

                     //cars_previous_vx[i] = v_x;
                     //cars_previous_vy[i] = v_y;
                 }
                 else
                 {
                     //a_x = a_very_large_number;
                     //a_y = a_very_large_number;

                     //cars_previous_vx.Add(v_x);
                     //cars_previous_vy.Add(v_y);
                 }
                 */

                 result += "[" + car_id + "," + x + "," + y + "," + v_x + "," + v_y + "," + frenet_values[0] + "," + frenet_values[1] + "," + a_x + "," + a_y + "]";
                //result += "[" + car_id + "," + x + "," + y + "," + v_x + "," + v_y + "," + frenet_values[0] + "," + frenet_values[1] + "]";

                



                car_id++;
            }

            /*
            for(int i=0;i<cars.Count;i++)
            {
                double a_x, a_y, dt = 0.02F;
                double a_very_large_number = 4096;

                double v_x = cars[i].GetComponent<Rigidbody>().velocity.x;
                double v_y = cars[i].GetComponent<Rigidbody>().velocity.z;

                CarAIControl carAI = (CarAIControl)cars[i].GetComponent(typeof(CarAIControl));

                if (car_id > 0)
                {
                    result += ",";
                }

                List<float> frenet_values = carAI.getThisFrenetFrame();

                if (System.Single.IsNaN(frenet_values[0]))
                {
                    frenet_values[0] = 0;
                }
                if (System.Single.IsNaN(frenet_values[1]))
                {
                    frenet_values[1] = 0;
                }


                //Debug.Log (car.transform.position.x+","+car.transform.position.z+","+frenet_values[0]+","+frenet_values[1]);

                if (cars_previous_vx.Count == cars.Count && cars_previous_vy.Count == cars.Count)
                {
                    a_x = (v_x - cars_previous_vx[i])/dt;
                    a_y = (v_y - cars_previous_vy[i])/dt;
                }
                else
                {
                    a_x = a_very_large_number;
                    a_y = a_very_large_number;
                }

                result += "[" + car_id + "," + cars[i].transform.position.x + "," + cars[i].transform.position.z + "," + v_x
                    + "," + v_y + "," + frenet_values[0] + "," + frenet_values[1] + "]";




                cars_previous_vx[i] = v_x;
                cars_previous_vy[i] = v_y;

                car_id++;


            }

            */
            /*
            foreach (GameObject car in cars) 
            {
                CarAIControl carAI = (CarAIControl) car.GetComponent(typeof(CarAIControl));

                //List<float> test_values = carAI.getFrenetFrame (1682.316f,2968.043f);

                //Debug.Log ("test values "+test_values [0] + "," + test_values [1]);

                if(car_id > 0)
                {
                    result += ",";
                }

                List<float> frenet_values = carAI.getThisFrenetFrame ();

                if(System.Single.IsNaN(frenet_values[0]))
                {
                    frenet_values[0] = 0;
                }
                if(System.Single.IsNaN(frenet_values[1]))
                {
                    frenet_values[1] = 0;
                }


                //Debug.Log (car.transform.position.x+","+car.transform.position.z+","+frenet_values[0]+","+frenet_values[1]);

                result += "[" + car_id + "," + car.transform.position.x + "," + car.transform.position.z +","+ car.GetComponent<Rigidbody> ().velocity.x 
                    +","+ car.GetComponent<Rigidbody>().velocity.z+","+frenet_values[0]+","+frenet_values[1]+"]";

                car_id++;
            }
            */


                result += "]";
			return result;
	}

}
}
