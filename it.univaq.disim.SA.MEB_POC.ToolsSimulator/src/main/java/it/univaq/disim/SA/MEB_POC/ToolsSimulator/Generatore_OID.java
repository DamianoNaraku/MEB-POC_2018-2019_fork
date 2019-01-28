package it.univaq.disim.SA.MEB_POC.ToolsSimulator;

import java.util.Random;

//<equip_OID> 0x 9C 98 50 24 D1 08 00 80</equip_OID>
//<recipe_OID>0x 0C 99 50 24 G1 00 00 80</recipe_OID>
//<step_OID>  0x AA 95  0 24 G1 00 00 90</ step_OID >
//0x 9 C 985024 D 108 0080
public class Generatore_OID {
	private String prefix = "0x";
	
	/*private int SerieNumero = 0;
	private String SerieLettera = "A";
	private String ToolFirstNumero = "000000";
	private String ToolLettera = "A";
	private String ToolNumeroSecond = "000";
	private String RecipeId = "0000";*/
	
	static final private String ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	static final private String NUMBERS = "0123456789";
		
	public String Generate_Recipe_OID() {
		String ToGenerate = prefix + random_string (1,NUMBERS) + 
				 random_string (1,ALPHABET) + 
				 random_string (6,NUMBERS) + 
				 random_string (1,ALPHABET) + 
				 random_string (3,NUMBERS) + 
				 random_string (4,NUMBERS);
		return ToGenerate;
	}
	
	public String Generate_Equip_OID(String suffix){
		String ToGenerate = prefix + 
				 random_string (1,NUMBERS) + 
				 random_string (1,ALPHABET) + 
				 random_string (6,NUMBERS) + 
				 random_string (1,ALPHABET) + 
				 random_string (3,NUMBERS) + 
				 suffix;
		return ToGenerate;
	}
	
	public String GenerateExaCode(int n_digit) {
		String esa_Alphabet = ALPHABET.substring(0,6);
		return random_string(n_digit,esa_Alphabet);
	}
	
	public String GenerateStepId(String suffix){
		String ToGenerate = prefix + random_string (1,NUMBERS) + 
				 random_string (2,ALPHABET) + 
				 random_string (6,NUMBERS) + 
				 random_string (1,ALPHABET) + 
				 random_string (2,NUMBERS) + 
				 suffix;
		return ToGenerate;
	}
	
	public String return_Prefix() {
		return prefix;
	}
	
	public String get_random_letter(int n_digit) {
		return random_string(n_digit,ALPHABET);
	}
	
	public String get_random_number(int n_digit) {
		return random_string(n_digit,NUMBERS);
	}
	
	public String random_string (int n_digit,String base) {
		String randomCode = "";
		for (int i = 0;i < n_digit;i++) {
			randomCode = randomCode + base.charAt(new Random().nextInt(base.length()));
		}
		return randomCode;
	}
}

