# Center a servo motor.  This script is used to find the exact center of a servo's range
# before mounting it into a robotic rig.  You may modify it if you need to set the motor
# to its left or right limits as well, but generally you can just turn it by hand.

import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)

pin_number = 11
GPIO.setup(pin_number, GPIO.OUT)
frequency_hertz = 50
ms_per_cycle = 1000 / frequency_hertz

# Determine the proper values for left_position and right_position using the calibservo.py 
# program, then plug them in here.  0.40 and 2.5 are the left and right limits for my
# TowerPro SG90 motors.
left_position = 0.40
right_position = 2.5
middle_position = (right_position - left_position) / 2 + left_position

# Center the motor.  
position = middle_position
pwm = GPIO.PWM(pin_number, frequency_hertz)
duty_cycle_percentage = position * 100 / ms_per_cycle
print("Centering at position: " + str(position))
print("Duty Cycle: " + str(duty_cycle_percentage))
print("")
pwm.start(duty_cycle_percentage)
time.sleep(.5)
	
# Turn everything off.
pwm.stop()
GPIO.cleanup()