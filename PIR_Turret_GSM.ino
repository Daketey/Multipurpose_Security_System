#include<Servo.h>
#include<Metro.h>
#include <SoftwareSerial.h>

SoftwareSerial sim(2, 3);
int _timeout;
String _buffer;
String number = ""; //-> change with your number

String prin;

int inputpin= 11;
int pirstate = LOW;
int pinvalue= 0;
Servo serX;
Servo serY;

unsigned long startMillis;
unsigned long currentMillis;
const unsigned long period = 3000;



String serialData;

void setup() {
  pinMode(inputpin , INPUT);
  serX.attach(6);
  serY.attach(10);
  Serial.begin(9600);
  Serial.setTimeout(10);
  _buffer.reserve(50);
  sim.begin(115200);
  startMillis = millis();
  prin="O1";
  serX.write(160);
  serY.write(50);
}

void loop() {
   
  pinvalue= digitalRead(inputpin);
  if(pinvalue==HIGH)
  {
    if(pirstate==LOW)
    {
      Serial.println("1");
      Serial.write(1);
      pirstate=HIGH;
   }}
     else {    
    if (pirstate == HIGH){
      Serial.println("0");
      Serial.write(0);
      pirstate = LOW;}
    } 

   currentMillis = millis();
   if(currentMillis - startMillis >= period && prin=="N1")
  { 
    callNumber();
    startMillis = currentMillis;
    prin="O1";
  }   
   }

void serialEvent()
{
  serialData = Serial.readString();
  
  if(serialData != "N1") {
  serX.write(parseDataX(serialData));
  serY.write(parseDataY(serialData));}

  else
  {
    prin=serialData;
    } 
 }
  
    

int parseDataX(String data){
  data.remove(data.indexOf("Y"));
  data.remove(data.indexOf("X"), 1);
  return data.toInt();
}

int parseDataY(String data){
  data.remove(0,data.indexOf("Y") + 1);
  return data.toInt();
}


void callNumber() {
  Serial.println("System Started...");
  sim.print (F("ATD"));
  sim.print (number);
  sim.print (F(";\r\n"));
  _buffer = _readSerial();
  Serial.println(_buffer);}

String _readSerial() {
  if (sim.available()) {
    return sim.read();
  }}
  

  
