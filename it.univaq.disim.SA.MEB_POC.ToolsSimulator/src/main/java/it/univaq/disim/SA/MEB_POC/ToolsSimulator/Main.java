package it.univaq.disim.SA.MEB_POC.ToolsSimulator;

import java.util.List;
import java.util.Random;
import java.util.stream.Collectors;

import org.apache.commons.lang3.time.StopWatch;

public class Main {
	public static void main(String[] args) {
		StopWatch stopwatch = new StopWatch();

		List<String> equipOIDs = Utilities.import_Equips_From_File();
		List<String> recipeOIDs = Utilities.import_Recipes_From_File();

		Counter messageCounter = new Counter();
		stopwatch.start();

		System.out.println("Starting tools! It will take 30 seconds...");

		for (int i = 0; i < equipOIDs.size(); i++) {

			String equipOID = equipOIDs.get(i);
			String category = equipOID.substring(equipOID.length() - 4);

			List<String> compatibleRecipes = recipeOIDs.stream()
					.filter(recipeOID -> category.equalsIgnoreCase(recipeOID.substring(recipeOID.length() - 4)))
					.collect(Collectors.toList());

			String recipeOID = compatibleRecipes.get(new Random().nextInt(compatibleRecipes.size()));

			ToolThread tool = new ToolThread(equipOID, recipeOID, messageCounter);
			tool.start();

			try {
				Thread.sleep(30);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}

		while (true) {

			if (stopwatch.getTime() > 60000) {
				System.out.println(
						"Current sending frequence: " + messageCounter.getValue() + " messages every 30 minute.");
				messageCounter.setValue(0);
				stopwatch.reset();
			}

			try {
				Thread.sleep(1000);
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}

}
