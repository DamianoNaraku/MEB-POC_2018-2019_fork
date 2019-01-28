package it.univaq.disim.SA.MEB_POC.ToolsSimulator.Models;

import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;

@XmlRootElement(name="InhibitEvent", namespace="http://www.w3.org/2001/XMLSchema-instance")
@XmlType(propOrder = {"equip_OID", "recipe_OID", "step_OID", "hold_type", "hold_flag", "event_datetime"})
public class Deleted {
	
	private String equip_OID;
	
	private String recipe_OID;
	
	private String step_OID;
	
	private String hold_type;
	
	private String hold_flag;
	
	private String event_datetime;
		
	public String getEquip_OID() {
		return equip_OID;
	}

	public void setEquip_OID(String Equip_OID) {
		this.equip_OID = Equip_OID;
	}

	public String getRecipe_OID() {
		return recipe_OID;
	}

	public void setRecipe_OID(String Recipe_OID) {
		this.recipe_OID = Recipe_OID;
	}

	public String getStep_OID() {
		return step_OID;
	}

	public void setStep_OID(String Step_OID) {
		this.step_OID = Step_OID;
	}

	public String getHold_type() {
		return hold_type;
	}

	public void setHold_type(String Hold_type) {
		this.hold_type = Hold_type;
	}

	public String getHold_flag() {
		return hold_flag;
	}

	public void setHold_flag(String Hold_flag) {
		this.hold_flag = Hold_flag;
	}
	
	public String getEvent_datetime() {
		return event_datetime;
	}

	public void setEvent_datetime(String eventDatetime) {
		this.event_datetime = eventDatetime;
	}
	
}
