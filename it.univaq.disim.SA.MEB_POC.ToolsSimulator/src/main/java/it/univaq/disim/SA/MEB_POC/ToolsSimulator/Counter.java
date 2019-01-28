package it.univaq.disim.SA.MEB_POC.ToolsSimulator;

public class Counter {

	private int value = 0;

    public int getValue() {
		return value;
	}

	public void setValue(int value) {
		this.value = value;
	}

	public synchronized int increment() {
        return value++;
    }
    
}
