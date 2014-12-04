//using UnityEngine;
//using System.Collections;
//
//
//	public class Button{
//		public Vector3 position;
//		public Vector2 size;
//		public string text;
//
//		public Button(Rect PositionSize, string Content){
//			position = new Vector3(PositionSize.x, PositionSize.y,1);
//			size = new Vector2(PositionSize.width, PositionSize.height);
//			text = Content;
//		}
//		
//		public Rect PositionSize{
//			get{
//				return new Rect(position.x, position.y, size.x, size.y);
//			}
//			set{
//				position = new Vector2(value.x, value.y);
//				size = new Vector2(value.width, value.height);
//			}
//		}
//		
//		delegate void ClickEventHandler();
//		ClickEventHandler OnClick;
//		
//		
//		
//	}
//
