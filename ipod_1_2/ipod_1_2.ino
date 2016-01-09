byte mode2[] = {0xFF, 0x55, 0x03, 0x00, 0x01, 0x02, 0xFA}; //mode 2 command
byte nobutton[] = {0xFF, 0x55, 0x03, 0x02, 0x00, 0x00, 0xFB}; //button release command
int release = 200;
//commands
byte playxpause = 0x01;
byte nextsong = 0x08;
byte prevsong = 0x10;
byte shuffle[] = {0xFF, 0x55, 0x04, 0x02, 0x00, 0x00, 0x80, 0x7A};
int long time;//
int butbefore = 1023;
int dockbef = 0;

void setup()
{
  Serial.begin(19200);  //sets serial com
  for (int p = 0; p < 7; p++) {
    Serial.print(mode2[p], HEX); //sends mode2 command
  }
}

void loop()
{
  int dock = analogRead(4);
  if (dock != dockbef) { //checks if there is an iPod conected
    for (int p = 0; p < 7; p++) { //if not, it sends again mode2 command
      Serial.print(mode2[p], HEX);
    }
  }
  int butnow = analogRead(0);
  if (butnow != butbefore && millis() - time > release) {
    if (butnow != HIGH) { //wait for button pressed
      time = millis();
      if (butnow == 0) {
        srlcommand(playxpause);
      }
      if (butnow > 948 && butnow < 953) {
        srlcommand(nextsong);
      }
      if (butnow >= 506 && butnow < 508) {
        srlcommand(prevsong);
      }
      if (butnow > 955 && butnow < 963) {
        for (int d = 0; d < 8; d++) {
          Serial.print(shuffle[d], BYTE);
        }
      }
    }
  }
  else
  {
    for (int d = 0; d < 7; d++) {
      Serial.print(nobutton[d], BYTE); //no button pressed command
    }
  }
  butbefore = butnow;
}

void srlcommand(byte select)
{
  byte checksum = 0x00 - 0x03 - 0x02 - 0x00 - select; //checksum of lenght, mode and command
  byte inst[] = {0xFF, 0x55, 0x03, 0x02, 0x00, select, checksum}; //general structure
  for (int m = 0; m < 7; m++) {
    Serial.print(inst[m], BYTE); //sends the command
  }
}


