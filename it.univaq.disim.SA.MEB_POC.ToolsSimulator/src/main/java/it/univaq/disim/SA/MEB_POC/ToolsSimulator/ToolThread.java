package it.univaq.disim.SA.MEB_POC.ToolsSimulator;

import java.util.Random;

import it.univaq.disim.SA.MEB_POC.ToolsSimulator.Models.InhibitEvent;

public class ToolThread extends Thread {

	private String equipOID;
	private String recipeOID;
	private Counter messageCounter;

	public ToolThread(String equipOID, String recipeOID, Counter messageCounter) {
		this.equipOID = equipOID;
		this.recipeOID = recipeOID;
		this.messageCounter = messageCounter;
	}

	@Override
	public void run() {

		while (true) {
			
			try {
				Thread.sleep(new Random().nextInt(240000));
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			
			InhibitEvent holdON = Utilities.Generate_Inhibit_Event(equipOID, recipeOID);

			String messaggeON = Utilities.Inhibit_to_XML(holdON);
	
			Utilities.broadcast(messaggeON);
			messageCounter.increment();
			
			try {
				Thread.sleep(new Random().nextInt(10000));
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
	
			InhibitEvent holdOFF = Utilities.Generate_Inverted_Inhibit(holdON);
	
			String messaggeOFF = Utilities.Inhibit_to_XML(holdOFF);
			Utilities.broadcast(messaggeOFF);
			messageCounter.increment();
		}
		
	}
}
