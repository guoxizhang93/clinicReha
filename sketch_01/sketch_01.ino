#include <Arduino.h>

const int actuatorPins[4] = {2, 3, 4, 5};
const int actuatorFeedbackPins[4] = {A0, A1, A2, A3};
const int pressurePins[8] = {A4, A5, A6, A7, A8, A9, A10, A11};

const int thighInflatePin = 6;
const int thighHoldPin = 7;
const int thighDeflatePin = 8;
const int calfInflatePin = 9;
const int calfHoldPin = 10;
const int calfDeflatePin = 11;

int actuatorPwm = 0;
bool feedbackEnabled = false;
unsigned long lastFeedbackMs = 0;

void setup() {
  Serial.begin(9600);
  for (int i = 0; i < 4; i++) {
    pinMode(actuatorPins[i], OUTPUT);
    pinMode(actuatorFeedbackPins[i], INPUT);
    analogWrite(actuatorPins[i], 0);
  }
  for (int i = 0; i < 8; i++) {
    pinMode(pressurePins[i], INPUT);
  }
  pinMode(thighInflatePin, OUTPUT);
  pinMode(thighHoldPin, OUTPUT);
  pinMode(thighDeflatePin, OUTPUT);
  pinMode(calfInflatePin, OUTPUT);
  pinMode(calfHoldPin, OUTPUT);
  pinMode(calfDeflatePin, OUTPUT);
  stopAirCuffs();
}

void loop() {
  while (Serial.available() > 0) {
    handleCommand((char)Serial.read());
  }
  if (feedbackEnabled && millis() - lastFeedbackMs >= 50) {
    sendFeedback();
    lastFeedbackMs = millis();
  }
}

void handleCommand(char command) {
  switch (command) {
    case '1':
    case '7':
      actuatorPwm = 0;
      applyActuatorPwm();
      Serial.println("actuator_zero");
      break;
    case '2':
      rampActuator(0, 100, 10);
      Serial.println("stretch_once");
      break;
    case '3':
      actuatorPwm = constrain(actuatorPwm + 10, 0, 115);
      applyActuatorPwm();
      Serial.println("actuator_plus");
      break;
    case '4':
      actuatorPwm = constrain(actuatorPwm - 10, 0, 115);
      applyActuatorPwm();
      Serial.println("actuator_minus");
      break;
    case '5':
      setThighAir(true, false, false);
      Serial.println("thigh_inflate");
      break;
    case '6':
      setThighAir(false, true, false);
      Serial.println("thigh_hold");
      break;
    case '8':
      setCalfAir(true, false, false);
      Serial.println("calf_inflate");
      break;
    case '9':
      setCalfAir(false, true, false);
      Serial.println("calf_hold");
      break;
    case 'q':
      setCalfAir(false, false, true);
      Serial.println("calf_deflate");
      break;
    case 'o':
      feedbackEnabled = true;
      Serial.println("feedback_on");
      break;
    case 'c':
      feedbackEnabled = false;
      stopAirCuffs();
      Serial.println("feedback_off");
      break;
    default:
      Serial.println("error");
      break;
  }
}

void applyActuatorPwm() {
  for (int i = 0; i < 4; i++) {
    analogWrite(actuatorPins[i], actuatorPwm);
  }
}

void rampActuator(int startPwm, int endPwm, int delayMs) {
  if (endPwm >= startPwm) {
    for (int pwm = startPwm; pwm <= endPwm; pwm += 2) {
      actuatorPwm = pwm;
      applyActuatorPwm();
      delay(delayMs);
    }
  } else {
    for (int pwm = startPwm; pwm >= endPwm; pwm -= 2) {
      actuatorPwm = pwm;
      applyActuatorPwm();
      delay(delayMs);
    }
  }
}

void setThighAir(bool inflate, bool hold, bool deflate) {
  digitalWrite(thighInflatePin, inflate ? HIGH : LOW);
  digitalWrite(thighHoldPin, hold ? HIGH : LOW);
  digitalWrite(thighDeflatePin, deflate ? HIGH : LOW);
}

void setCalfAir(bool inflate, bool hold, bool deflate) {
  digitalWrite(calfInflatePin, inflate ? HIGH : LOW);
  digitalWrite(calfHoldPin, hold ? HIGH : LOW);
  digitalWrite(calfDeflatePin, deflate ? HIGH : LOW);
}

void stopAirCuffs() {
  setThighAir(false, true, false);
  setCalfAir(false, true, false);
}

void sendFeedback() {
  Serial.print("pos:");
  for (int i = 0; i < 4; i++) {
    Serial.print(analogRead(actuatorFeedbackPins[i]));
    if (i < 3) Serial.print(',');
  }
  Serial.print(";press:");
  for (int i = 0; i < 8; i++) {
    Serial.print(analogRead(pressurePins[i]));
    if (i < 7) Serial.print(',');
  }
  Serial.println(";stop:s t o p");
}
