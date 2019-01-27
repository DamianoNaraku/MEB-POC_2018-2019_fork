package it.univaq.disim.SA.MEB_POC.StreamProcessor;

import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlType;


@XmlRootElement(name="InhibitEvent")
@XmlType(propOrder = {"inserted", "deleted"})
public class InhibitEvent {
	Inserted in;
	Deleted del;

	public Inserted getInserted() {
		return in;
	}

	@XmlElement(name="Inserted")
	public void setInserted(Inserted in) {
		this.in = in;
	}

	public Deleted getDeleted() {
		return del;
	}

	@XmlElement(name="Deleted")
	public void setDeleted(Deleted del) {
		this.del = del;
	}
}
