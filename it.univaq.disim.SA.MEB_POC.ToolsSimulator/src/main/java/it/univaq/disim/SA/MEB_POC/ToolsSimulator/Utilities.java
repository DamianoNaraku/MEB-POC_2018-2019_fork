package it.univaq.disim.SA.MEB_POC.ToolsSimulator;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.StringWriter;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import java.net.UnknownHostException;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Random;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.Marshaller;

import it.univaq.disim.SA.MEB_POC.ToolsSimulator.Models.Deleted;
import it.univaq.disim.SA.MEB_POC.ToolsSimulator.Models.InhibitEvent;
import it.univaq.disim.SA.MEB_POC.ToolsSimulator.Models.Inserted;

public class Utilities {

	public static InetAddress broadcastAdress = staticInit();
	public static DatagramSocket datagramsocket;
	public static String broadcastStr = "192.168.1.255";
	public static int broadcastPort = 20001;

	public static void broadcast(String s) {
		try {
			broadcast0(s);
		} catch (IOException ex) {
			System.out.println("broadcast Failed:" + ex.toString());
		}
	}

	public static void broadcast0(String s) throws SocketException, IOException {
		byte[] data = s.getBytes();
		datagramsocket.setBroadcast(true);
		DatagramPacket datagram = new DatagramPacket(data, data.length, broadcastAdress, broadcastPort);
		datagramsocket.send(datagram);
	}

	public static InetAddress staticInit() {
		try {
			datagramsocket = new DatagramSocket();
		} catch (SocketException ex) {
			System.out.println("failed to create datagramsocket: " + ex.toString());
		}
		try {
			broadcastAdress = InetAddress.getByName("255.255.255.255");
		} catch (UnknownHostException e) {
			System.out.println("failed to se broadcast adress: " + e.toString());
		}
		return broadcastAdress;
	}

	public static InhibitEvent Generate_Inhibit_Event(String equipOID, String recipeOID) {
		InhibitEvent inhibitEvent = new InhibitEvent();

		Inserted inserted = new Inserted();
		Deleted del = new Deleted();

		/*if (new Random().nextInt(100) != 1) {
			inserted.setEquip_OID(equipOID);
			del.setEquip_OID(equipOID);
		} else {
			inserted.setEquip_OID("");
			del.setEquip_OID("");
		}*/
		inserted.setEquip_OID("");
		del.setEquip_OID("");
		
		inserted.setRecipe_OID(recipeOID);
		del.setRecipe_OID(recipeOID);

		inserted.setStep_OID(new Generatore_OID().GenerateStepId(equipOID.substring(equipOID.length() - 4)));
		del.setStep_OID(inserted.getStep_OID());

		inserted.setHold_type("ProcessEquipHold_" + new Random().nextInt(50));
		del.setHold_type(inserted.getHold_type());

		inserted.setHold_flag(new Random().nextInt(1) == 0 ? "N" : "Y");
		del.setHold_flag(inserted.getHold_flag().equalsIgnoreCase("N") ? "Y" : "N");

		DateFormat df = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS");
		inserted.setEvent_datetime(df.format(new Date()));
		del.setEvent_datetime(inserted.getEvent_datetime());

		inhibitEvent.setInserted(inserted);
		inhibitEvent.setDeleted(del);

		return inhibitEvent;

	}

	public static InhibitEvent Generate_Inverted_Inhibit(InhibitEvent Original_Inhibit) {

		InhibitEvent Inverted_Inhibit = new InhibitEvent();

		Inserted Inverted_Inserted = new Inserted();
		Deleted Inverted_Deleted = new Deleted();

		Inverted_Inserted.setEquip_OID(Original_Inhibit.getInserted().getEquip_OID());
		Inverted_Deleted.setEquip_OID(Original_Inhibit.getDeleted().getEquip_OID());

		Inverted_Inserted.setRecipe_OID(Original_Inhibit.getInserted().getRecipe_OID());
		Inverted_Deleted.setRecipe_OID(Original_Inhibit.getDeleted().getRecipe_OID());

		Inverted_Inserted.setStep_OID(Original_Inhibit.getInserted().getStep_OID());
		Inverted_Deleted.setStep_OID(Original_Inhibit.getDeleted().getStep_OID());

		Inverted_Inserted.setHold_type(Original_Inhibit.getInserted().getHold_type());
		Inverted_Deleted.setHold_type(Original_Inhibit.getDeleted().getHold_type());

		String OldInsHoldFlag = Original_Inhibit.getInserted().getHold_flag();
		String NewInsHoldFlag = (OldInsHoldFlag.equals("Y") ? "N" : "Y");
		String OldDelHoldFlag = Original_Inhibit.getDeleted().getHold_flag();
		String NewDelHoldFlag = (OldDelHoldFlag.equals("Y") ? "N" : "Y");

		Inverted_Inserted.setHold_flag(NewInsHoldFlag);
		Inverted_Deleted.setHold_flag(NewDelHoldFlag);

		DateFormat df = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS");
		Inverted_Inserted.setEvent_datetime(df.format(new Date()));
		Inverted_Deleted.setEvent_datetime(Inverted_Inserted.getEvent_datetime());

		Inverted_Inhibit.setInserted(Inverted_Inserted);
		Inverted_Inhibit.setDeleted(Inverted_Deleted);

		return Inverted_Inhibit;

	}

	public static String Inhibit_to_XML(InhibitEvent event) {
		
		try {

			JAXBContext jaxbContext = JAXBContext.newInstance(InhibitEvent.class);
			Marshaller jaxbMarshaller = jaxbContext.createMarshaller();
			jaxbMarshaller.setProperty(Marshaller.JAXB_FORMATTED_OUTPUT, true);

			StringWriter message = new StringWriter();
			jaxbMarshaller.marshal(event, message);

			return message.toString();
		} catch (Exception e) {
			e.printStackTrace();
		}

		return "No message";
	}

	public static List<String> import_Equips_From_File() {
		List<String> Lista = new ArrayList<String>();

		InputStream res = Main.class.getResourceAsStream("/EquipList.txt");

		BufferedReader reader = new BufferedReader(new InputStreamReader(res));
		String line = null;

		try {
			while ((line = reader.readLine()) != null) {
				Lista.add(line.substring(0, line.indexOf(",")));
			}
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		try {
			reader.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		return Lista;
	}

	public static List<String> import_Recipes_From_File() {
		List<String> Lista = new ArrayList<String>();

		InputStream res = Main.class.getResourceAsStream("/RecipeList.txt");

		BufferedReader reader = new BufferedReader(new InputStreamReader(res));
		String line = null;

		try {
			while ((line = reader.readLine()) != null) {
				Lista.add(line.substring(0, line.indexOf(",")));
			}
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		try {
			reader.close();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		return Lista;
	}
}
