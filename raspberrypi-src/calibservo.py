# Calibrate a servo.  This script is used to find a given servo motor's left and right 
# positional limits.  These values are needed for all other servo scripts.  Most motors
# will work with standard values 0.75 ms to 2.5 ms, but they might whine and jitter or 
# you may not be taking advantage of your motor's maximum range.  This script lets you find
# values you can use to fine tune your programs to fit your motor.  See comments throughout.

import RPi.GPIO as GPIO
import time

GPIO.setmode(GPIO.BOARD)

pin_number = 11
GPIO.setup(pin_number, GPIO.OUT)
frequency_hertz = 50
ms_per_cycle = 1000 / frequency_hertz

# Play with these settings to find the best limits for your servo.
# For the TowerPro SG90 that we're using, I found 0.40 and 2.5 to be correct.
# For your own, you may want to start with 0.75 and 2.5.  I find the left 
# limit to be more inconsistent than the right limit.
left_position = 0.40
right_position = 2.5
middle_position = (right_position - left_position) / 2 + left_position
positionList = [left_position, right_position]

pwm = GPIO.PWM(pin_number, frequency_hertz)

for i in range(3):
	for position in positionList:
		duty_cycle_percentage = position * 100 / ms_per_cycle
		print("Position: " + str(position))
		print("Duty Cycle: " + str(duty_cycle_percentage))
		print("")
		pwm.start(duty_cycle_percentage)
		time.sleep(.5)
	

# Done.  Terminate all signals and relax the motor.
pwm.stop()

# We have shut all our stuff down but we should do a complete
# close on all GPIO stuff.  There's only one copy of real hardware.
# We need to be polite and put it back the way we found it.
GPIO.cleanup()

