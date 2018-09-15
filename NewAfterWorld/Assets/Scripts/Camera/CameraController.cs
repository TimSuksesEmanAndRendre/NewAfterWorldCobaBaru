using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CameraController:MonoBehaviour
{

	[HideInInspector]
	public bool movingBuilding = false;

	private bool 
		moveNb, 					//bools for button controlled moving Move North, East, South, West
		moveEb, 
		moveSb, 
		moveWb, 
		zoomInb, 
		zoomOutb,

		fadeMoveNb, 					//bools for fade out moving Move North, East, South, West
		fadeMoveSb, 
		fadeMoveEb, 
		fadeMoveWb, 
		fadeZoomInb, 
		fadeZoomOutb,

		fade = false,
		fadePan = false,
		fadeZoom = false,

		fadeTouch = false,
		//fadeTouchPan = false,
		//fadeTouchZoom = false,
		stillMovingTouch = false; 
		

	private float
		lastPanX,
		lastPanY,
		zoomMax = 1.0f,						//caps for zoom
		zoomMin = 0.25f,
		zoom, 								// the current camera zoom factor
		currentFingerDistance,
		previousFingerDistance,
		camStepDefault,
		zoomStepDefault;

	public float
		touchPanSpeed = 4.0f,				
		camStep = 15,
		zoomStep = 0.01f,					

		camStepFade = 0.3f,
		zoomStepFade = 0.00005f;
			 							

	private Vector3 
		//initPos = new Vector3(0,0,-10),
		RayHitPoint;
		
	private Vector2 
		velocity, 
		target;

	//[HideInInspector]
	public int 								//camera bounds
	northMax = 4200,	//4200,	
	southMin = -4300,  	//-4300	
	eastMax = 5200,	//5200		
	westMin = -5200;	//-5200	

	private tk2dCamera spriteLightKitCam;
	public GameObject SpriteLightKitCam;
	private Relay relay;
	private tk2dCamera cameratk2D;


	[SerializeField]
	private Camera cam;

	[SerializeField]
	private float speed	= 0.0f;
	private Vector3 Origin;
	private Vector3 Diference;
//	private Vector3 pos;
	private bool Drag=false;
	private Vector3 vel;

	void Awake() 
	{
		cam = GetComponent<Camera>();
		cameratk2D = gameObject.GetComponent<tk2dCamera>();
		relay = GameObject.Find ("Relay").GetComponent<Relay>();
		spriteLightKitCam = SpriteLightKitCam.GetComponent<tk2dCamera>();
		camStepDefault = camStep;
		zoomStepDefault = zoomStep;

//		InvokeRepeating("UpdateFunctionality", 0.0f, 0.01f);
	}

	void Start(){}

	//to prevent selecting the buildint underneath the move buttons

	private void Delay() { ((Relay)relay).DelayInput(); }

	//Move
	public void MoveN()	{ moveSb = false; if(!moveNb) moveNb = true; else StopMove(); Delay (); }	//FadeOutPan(); 
	public void MoveE()	{ moveWb = false; if(!moveEb) moveEb = true; else StopMove(); Delay (); }
	public void MoveS()	{ moveNb = false; if(!moveSb) moveSb = true; else StopMove(); Delay (); }
	public void MoveW()	{ moveEb = false; if(!moveWb) moveWb = true; else StopMove(); Delay (); }

	//FadeMove
	private void FadeMoveN() { moveSb = false; moveNb = true; fade = true; fadePan = true; camStep = camStepDefault;}	 
	private void FadeMoveS() { moveNb = false; moveSb = true; fade = true; fadePan = true; camStep = camStepDefault;}	
	private void FadeMoveE() { moveWb = false; moveEb = true; fade = true; fadePan = true; camStep = camStepDefault;}	 
	private void FadeMoveW() { moveEb = false; moveWb = true; fade = true; fadePan = true; camStep = camStepDefault;}   

	//Zoom
	public void ZoomIn() { zoomOutb = false; if(!zoomInb) zoomInb = true; else StopZoom(); Delay (); }//FadeOutZoom();
	public void ZoomOut() { zoomInb = false; if(!zoomOutb) zoomOutb = true;	else StopZoom(); Delay (); }

	//FadeZoom
	public void FadeZoomIn()  { zoomOutb = false; zoomInb = true;  fade = true; fadeZoom = true; zoomStep = zoomStepDefault; }	
	public void FadeZoomOut() { zoomInb = false;  zoomOutb = true; fade = true; fadeZoom = true; zoomStep = zoomStepDefault; }

	// conditions keep the camera from going off-map, leaving a margin for zoom-in/out

	private void MoveCam(float speedX,float speedY){transform.position += new Vector3 (speedX, speedY, 0) ;}
		
//	void UpdateFunctionality()
	void LateUpdate()
	{	
		//if (((Relay)relay).pauseInput)//if a menu panel is displayed on screen, stop scroll, zoom etc.
		//				return;


		if(fade)
		{
			if(fadePan)
			{
				camStep-= camStepFade;
				if(camStep<=0)
				{
					StopMove();
					camStep = camStepDefault;
					fadePan = false;
				}
			}
			if(fadeZoom)
			{
				zoomStep-= zoomStepFade;
				if(zoomStep<=0.008)//0.002
				{
					StopZoom();
					zoomStep = zoomStepDefault;
					fadeZoom = false;
				}
			}

			if(!fadePan&&!fadeZoom)
				fade = false;
		}
		MoveCamera();
		MouseZoom ();
		TouchMoveZoom ();
		ButtonMoveZoom ();
		UpdateSpriteLightKitCam ();
	}

	/// <summary>
	/// Moves the camera.
	/// </summary>
	private void MoveCamera()
	{
		if(Input.GetMouseButton (0) && Input.mousePosition.x < Screen.width && Input.mousePosition.y < Screen.height)
		{
			Diference = (cam.ScreenToWorldPoint(Input.mousePosition)) - cam.transform.position;
			if(Drag == false)
			{
				Drag = true;
				Origin = cam.ScreenToWorldPoint (Input.mousePosition);
			}
		}
		else
		{
			Drag=false;
		}

		if (Drag) {
			Vector2 pos = new Vector3 ((Origin - Diference).x, (Origin - Diference).y);
			float x = Mathf.Clamp (pos.x, westMin, eastMax);
			float y = Mathf.Clamp (pos.y, southMin, northMax);
			pos = new Vector2 (x, y);
			cam.transform.position = Vector3.SmoothDamp (cam.transform.position, pos, ref vel, Time.smoothDeltaTime * speed);
		}
	}

	private void UpdateSpriteLightKitCam()
	{
		SpriteLightKitCam.transform.position = transform.position;
		spriteLightKitCam.ZoomFactor = cameratk2D.ZoomFactor;
	}
	private void MouseZoom()
	{

		if (Input.GetAxis("Mouse ScrollWheel")<0) 
		{
			zoomInb = false; if(!zoomOutb) zoomOutb = true;
		}
		else if (Input.GetAxis("Mouse ScrollWheel") > 0) 
		{
			zoomOutb = false; if(!zoomInb) zoomInb = true; 
		}
		/*
		else
		{
			StopZoom();  //this cancels button zoom commands
		}
		*/
	}
	private void ButtonMoveZoom()
	{
		//NSEW distance bounds

		if(moveNb && transform.position.y < northMax){MoveCam(0,camStep); ResetFadePan(); fadeMoveNb = true; } 		// N
		else if(moveSb && transform.position.y > southMin){MoveCam(0,-camStep); ResetFadePan(); fadeMoveSb = true; }	// S
				
		if (moveEb && transform.position.x < eastMax){MoveCam(camStep,0); ResetFadePan(); fadeMoveEb = true; } 		// E
		else if (moveWb && transform.position.x > westMin){MoveCam(-camStep,0); ResetFadePan();fadeMoveWb = true; }	// W

		zoom = cameratk2D.ZoomFactor;		

		//zoom in/out

		if(zoomInb && zoom<zoomMax)
		{
			cameratk2D.ZoomFactor += zoomStep;   //In
			fadeZoomOutb = false; fadeZoomInb = true; 
		}
		else if(zoomOutb && zoom>zoomMin)
		{
			cameratk2D.ZoomFactor -= zoomStep;	//Out
			fadeZoomInb = false; fadeZoomOutb = true;
		}
	}

	private void TouchMoveZoom()
	{
		zoom = cameratk2D.ZoomFactor;

		if (Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Moved//chech for 2 fingers on screen
				&& Input.GetTouch(1).phase == TouchPhase.Moved) 
		{
			if(!((Relay)relay).delay) Delay ();
			Vector2 touchPosition0 = Input.GetTouch(0).position;//positions for both fingers for pinch zoom in/out
			Vector2 touchPosition1 = Input.GetTouch(1).position;
				
			currentFingerDistance = Vector2.Distance(touchPosition0,touchPosition1);//distance between fingers
				
			//AUTO ZOOM - stopped with tap and brief hold
			/*
			if (currentFingerDistance>previousFingerDistance && zoom<zoomMax)
			{
				if(!ZoomInb)
					{
						ZoomOutb = false;
						ZoomInb = true;
					}
			}
			else if(zoom>zoomMin)
			{
				if(!ZoomOutb)
				{
					ZoomInb = false;
					ZoomOutb = true;			 
				}
			}
			*/

			//MANUAL ZOOM

			if (currentFingerDistance>previousFingerDistance)
			{
				if(zoom<zoomMax)
				{
					cameratk2D.ZoomFactor += zoomStep;
					fadeZoomOutb = false; fadeZoomInb = true; FadeZoomIn();
				}
			}

			else if(currentFingerDistance<previousFingerDistance && zoom>zoomMin)
			{
				cameratk2D.ZoomFactor -= zoomStep;
				fadeZoomInb = false; fadeZoomOutb = true; FadeZoomOut(); 
			}

			previousFingerDistance = currentFingerDistance;
		
		}
		else if (Input.touchCount ==1 && Input.GetTouch(0).phase == TouchPhase.Stationary)// 
		{
			StopAll ();		
		}

	}

	private void ResetFadePan()
	{
		fadeMoveNb=false;
		fadeMoveSb=false;
		fadeMoveEb=false;
		fadeMoveWb=false;
	}

	private void FadeOutPan()
	{
		if(fadeMoveEb){FadeMoveE();fadeMoveEb=false;}
		else if(fadeMoveWb){FadeMoveW();fadeMoveWb=false;}

		if(fadeMoveNb){FadeMoveN();fadeMoveNb=false;}
		else if(fadeMoveSb){FadeMoveS();fadeMoveSb=false;}	
	}

	private void FadeOutPanTouch()
	{
		fadeTouch = true;
	}

	private void FadeOutZoom()
	{
		if(fadeZoomInb){FadeZoomIn();fadeZoomInb=false;} 
		else if(fadeZoomOutb){FadeZoomOut();fadeZoomOutb=false;}
	}

	private void StopAll()
	{
		moveNb=false; moveSb=false; moveEb=false; moveWb=false;
		zoomInb = false; zoomOutb = false;	
		fadeTouch = false;
	}

	private void StopMove()
	{
		moveNb=false; moveSb=false; moveEb=false; moveWb=false;
	}
	private void StopZoom()
	{
		zoomInb = false; zoomOutb = false;	
	}
}