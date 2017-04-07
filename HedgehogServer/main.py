import math
import time
from hedgehog.client import connect
import socket

SPEED = 500

def forward():
    hedgehog.move(0, SPEED)
    hedgehog.move(1, SPEED)

def back():
    hedgehog.move(0, -SPEED)
    hedgehog.move(1, -SPEED)

def left():
    hedgehog.move(0, -SPEED)
    hedgehog.move(1, SPEED)
        
def right():
    hedgehog.move(0, SPEED)
    hedgehog.move(1, -SPEED)

def stop():
    hedgehog.move(0, 0)
    hedgehog.move(1, 0)

def increaseSpeed():
    global SPEED
    if SPEED < 1000:
        SPEED = SPEED + 100
    printSpeed()

def decreaseSpeed():
    global SPEED
    if (SPEED > 0):
        SPEED = SPEED - 100
    printSpeed()
    
def setSpeed(newSpeed):
    global SPEED
    SPEED = newSpeed
    printSpeed()

def printSpeed():
    print("Current Speed: " + str(SPEED))

# Main
with connect() as hedgehog:
    stop()
    
    global SPEED
    
    IP = socket.gethostbyname(socket.gethostname())
    PORT = 3131
    BUFFER_SIZE = 1
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind((IP, PORT))
    
    while True:
        SPEED = 500
        
        print("Server listening")
        server.listen(5)
        
        connection, client_address = server.accept()
        print("[" + str(client_address[0]) + "] Connected")
        
        while True:
            try:
                # Receive data
                dataBytes = connection.recv(BUFFER_SIZE)
                if not dataBytes: break
                
                # Convert received bytes into int
                data = int.from_bytes(dataBytes, byteorder='big')
                print("[" + str(client_address[0]) + "] Received: " + str(data))
                
                # Do movement
                if data == -1 or data == 255:
                    stop()
                    break
                elif data == 0:
                   forward()
                elif data == 1:
                    left()
                elif data == 2:
                    back()
                elif data == 3:
                    right()
                elif data == 4:
                    increaseSpeed()
                elif data == 5:
                    decreaseSpeed()
                elif data == 6:
                    stop()
                elif data == 7:
                    stop()
                elif data == 31:
                    setSpeed(100)
                elif data == 32:
                    setSpeed(200)
                elif data == 33:
                    setSpeed(300)
                elif data == 34:
                    setSpeed(400)
                elif data == 35:
                    setSpeed(500)
                elif data == 36:
                    setSpeed(600)
                elif data == 37:
                    setSpeed(700)
                elif data == 38:
                    setSpeed(800)
                elif data == 39:
                    setSpeed(900)
                elif data == 40:
                    setSpeed(1000)
                
            except:
                print("[" + str(client_address[0]) + "] Disconnected")
                break