import tkinter as tk
import os, subprocess
usingRandom = None
debug = False

def setupWindow(window):
	global usingRandom
	usingRandom = {"red": tk.IntVar(), "green": tk.IntVar(), "blue": tk.IntVar()}
	# setup velocity frame
	if True:
		velocityFrame = tk.LabelFrame(window,text="Векторы скорости")
		xVelLabel = tk.Label(velocityFrame,text="Горизонтальная скорость (левая граница, правая граница)")
		xVelInp1 = tk.Entry(velocityFrame)
		xVelInp2 = tk.Entry(velocityFrame)
		yVelLabel = tk.Label(velocityFrame,text="Вертикальная скорость (левая граница, правая граница)")
		yVelInp1 = tk.Entry(velocityFrame)
		yVelInp2 = tk.Entry(velocityFrame)
		angVelLabel = tk.Label(velocityFrame,text="Угловая скорость (левая граница, правая граница)")
		angVelInp1 = tk.Entry(velocityFrame)
		angVelInp2 = tk.Entry(velocityFrame)

		velocityFrame.grid(row=0,column=1,columnspan=1)
		xVelLabel.grid(row=0,column=0,columnspan=2)
		xVelInp1.grid(row=1,column=0)
		xVelInp2.grid(row=1,column=1)
		yVelLabel.grid(row=2,column=0,columnspan=2)
		yVelInp1.grid(row=3,column=0)
		yVelInp2.grid(row=3,column=1)
		angVelLabel.grid(row=4,column=0,columnspan=2)
		angVelInp1.grid(row=5,column=0,pady=[0,8])
		angVelInp2.grid(row=5,column=1,pady=[0,8])	

	# setup additional parameters
	if True:
		particleFrame = tk.LabelFrame(window,text="Настройки частиц")
		sizeMultLabel = tk.Label(particleFrame, text="Множитель размера частиц в процентах(левая граница, правая граница)")
		sizeMultInp1 = tk.Entry(particleFrame)
		sizeMultInp2 = tk.Entry(particleFrame)
		maxParticlesLabel = tk.Label(particleFrame, text="Кол-во частиц")
		maxParticlesInp = tk.Entry(particleFrame)
		maxParticleResLabel = tk.Label(particleFrame, text="Максимальный размер частиц (в пикселях)")
		maxParticleResInp = tk.Entry(particleFrame)

		particleFrame.grid(row=1,column=0,columnspan=1)
		sizeMultLabel.grid(row=0,column=0,columnspan=2)
		sizeMultInp1.grid(row=1,column=0)
		sizeMultInp2.grid(row=1,column=1)
		maxParticlesLabel.grid(row=2,column=0)
		maxParticlesInp.grid(row=3,column=0, pady=[0,8])
		maxParticleResLabel.grid(row=2,column=1)
		maxParticleResInp.grid(row=3,column=1, pady=[0,8])

	# setup color settings and panels
	if True:
		colorFrame = tk.LabelFrame(window,text="Настройки цвета")
		colorBGLabel = tk.Label(colorFrame, text="Цвет заднего фона в RGB")
		colorBGInputR = tk.Entry(colorFrame)
		colorBGInputG = tk.Entry(colorFrame)
		colorBGInputB = tk.Entry(colorFrame)

		useRandomColorsLabel = tk.Label(colorFrame, text="Использовать случайные цвета для частиц?")
		useRandomColorsR = tk.Checkbutton(colorFrame, text="Красный", variable=usingRandom["red"])
		useRandomColorsG = tk.Checkbutton(colorFrame, text="Зеленый", variable=usingRandom["green"])
		useRandomColorsB = tk.Checkbutton(colorFrame, text="Синий", variable=usingRandom["blue"])

		colorFrame.grid(row=0,column=0,columnspan=1)
		colorBGLabel.grid(row=0,column=0,columnspan=3)
		colorBGInputR.grid(row=1,column=0)
		colorBGInputG.grid(row=1,column=1)
		colorBGInputB.grid(row=1,column=2)
		useRandomColorsLabel.grid(row=2,column=0,columnspan=3)
		useRandomColorsR.grid(row=3,column=0)
		useRandomColorsG.grid(row=4,column=0)
		useRandomColorsB.grid(row=5,column=0)

		# R panel
		redColorFrame = tk.LabelFrame(colorFrame,text="Красный")
		redColorLabel = tk.Label(redColorFrame,text="Границы случайности (0 - 255)")
		redColorInp1 = tk.Entry(redColorFrame)
		redColorInp2 = tk.Entry(redColorFrame)

		redColorFrame.grid(row=3,column=1,columnspan=2)
		redColorLabel.grid(row=0,column=0,columnspan=2)
		redColorInp1.grid(row=1,column=0)
		redColorInp2.grid(row=1,column=1)

		# G panel
		greenColorFrame = tk.LabelFrame(colorFrame,text="Зеленый")
		greenColorLabel = tk.Label(greenColorFrame,text="Границы случайности (0 - 255)")
		greenColorInp1 = tk.Entry(greenColorFrame)
		greenColorInp2 = tk.Entry(greenColorFrame)

		greenColorFrame.grid(row=4,column=1,columnspan=2)
		greenColorLabel.grid(row=0,column=0,columnspan=2)
		greenColorInp1.grid(row=1,column=0)
		greenColorInp2.grid(row=1,column=1)

		# B panel
		blueColorFrame = tk.LabelFrame(colorFrame,text="Синий")
		blueColorLabel = tk.Label(blueColorFrame,text="Границы случайности (0 - 255)")
		blueColorInp1 = tk.Entry(blueColorFrame)
		blueColorInp2 = tk.Entry(blueColorFrame)

		blueColorFrame.grid(row=5,column=1,columnspan=2)
		blueColorLabel.grid(row=0,column=0,columnspan=2)
		blueColorInp1.grid(row=1,column=0)
		blueColorInp2.grid(row=1,column=1)

		# default color setup
		defaultColorLabel = tk.Label(colorFrame, text="Стандартный цвет, если не выбран случайный (RGB)")
		defaultColorR = tk.Entry(colorFrame)
		defaultColorG = tk.Entry(colorFrame)
		defaultColorB = tk.Entry(colorFrame)

		defaultColorLabel.grid(row=6,column=0,columnspan=3)
		defaultColorR.grid(row=7,column=0)
		defaultColorG.grid(row=7,column=1)
		defaultColorB.grid(row=7,column=2)

	# after all is set, load config
	if not debug:
		path = os.path.abspath("./assets/config.txt")
	else:
		path = "./config.txt"
	config = open(path, "r").readlines()
	for line in config:
		words = line.split(" ")
		command = words[0]
		del words[0]
		if(command == "xvel"):
			xVelInp1.insert(0, words[0])
			xVelInp2.insert(0, words[1])
		elif(command == "yvel"):
			yVelInp1.insert(0, words[0])
			yVelInp2.insert(0, words[1])
		elif(command == "angvel"):
			angVelInp1.insert(0, words[0])
			angVelInp2.insert(0, words[1])
		elif(command == "sizemult"):
			sizeMultInp1.insert(0, words[0])
			sizeMultInp2.insert(0, words[1])
		elif(command == "pcount"):
			maxParticlesInp.insert(0, words[0])
		elif(command == "maxrectres"):
			maxParticleResInp.insert(0, words[0])
		elif(command == "bgcolor"):
			colorBGInputR.insert(0, words[0])
			colorBGInputG.insert(0, words[1])
			colorBGInputB.insert(0, words[2])
		elif(command == "defaultcolor"):
			defaultColorR.insert(0, words[0])
			defaultColorG.insert(0, words[1])
			defaultColorB.insert(0, words[2])
		elif(command == "userandom"):
			usingRandom["red"].set(int(words[0]))
			usingRandom["green"].set(int(words[1]))
			usingRandom["blue"].set(int(words[2]))
		elif(command == "rrandom"):
			redColorInp1.insert(0, words[0])
			redColorInp2.insert(0, words[1])
		elif(command == "grandom"):
			greenColorInp1.insert(0, words[0])
			greenColorInp2.insert(0, words[1])
		elif(command == "brandom"):
			blueColorInp1.insert(0, words[0])
			blueColorInp2.insert(0, words[1])
	# function to save config on button
	def saveSettings():
		errors = []
		warnings = []
		## values
		xVel = [xVelInp1.get(), xVelInp2.get()]
		yVel = [yVelInp1.get(), yVelInp2.get()]
		angVel = [angVelInp1.get(), angVelInp2.get()]
		pCount = maxParticlesInp.get()
		maxSize = maxParticleResInp.get()
		maxSizeMult = [sizeMultInp1.get(), sizeMultInp2.get()]
		randomColorsR = [redColorInp1.get(), redColorInp2.get()]
		randomColorsG = [greenColorInp1.get(), greenColorInp2.get()]
		randomColorsB = [blueColorInp1.get(), blueColorInp2.get()]
		defaultColor = [defaultColorR.get(), defaultColorG.get(), defaultColorB.get()]
		backgroundColor = [colorBGInputR.get(), colorBGInputG.get(), colorBGInputB.get()]
		## get all errors out
		if True:
			if not all(xVel):
				errors.append("Не введена горизонтальная скорость")
			if not all(yVel):
				errors.append("Не введена вертикальная скорость")
			if not all(angVel):
				errors.append("Не введена угловая скорость")
			if not pCount:
				errors.append("Не указано число частиц")
			if not all(maxSizeMult):
				errors.append("Не введены границы множителя размеров")

			if usingRandom["red"].get():
				if not all(randomColorsR):
					errors.append("Красный цвет случайный, но значения не выставленны")
			else:
				if not defaultColor[0]:
					errors.append("Не установленно стандарное значение красного цвета")

			if usingRandom["green"].get():
				if not all(randomColorsG):
					errors.append("Зеленый цвет случайный, но значения не выставленны")
			else:
				if not defaultColor[1]:
					errors.append("Не установленно стандарное значение зеленого цвета")

			if usingRandom["blue"].get():
				if not all(randomColorsR):
					errors.append("Синий цвет случайный, но значения не выставленны")
			else:
				if not defaultColor[2]:
					errors.append("Не установленно стандарное значение синего цвета")

			if not all(backgroundColor):
				errors.append("Цвет фона не установлен")
		## warning
		if not maxSize:
			warnings.append("Не указан максимальный размер частиц, картинки не будут соразмерны!")


		if(len(errors) == 0):
			if(len(warnings) != 0):
				warnWindow = tk.Toplevel()
				for warn in warnings:
					tk.Label(warnWindow,text=warn).pack()
				tk.Label(warnWindow, text="Это предупреждения: учтите их. Изменения будут внесены").pack()
			if not debug:
				path = "./assets/config.txt"
			else:
				path = "./config.txt"
			file = open(path, "w")
			file.write(f"xvel {xVel[0]} {xVel[1]}\n")
			file.write(f"yvel {yVel[0]} {yVel[1]}\n")
			file.write(f"angvel {angVel[0]} {angVel[1]}\n")
			file.write(f"sizemult {maxSizeMult[0]} {maxSizeMult[1]}\n")
			file.write(f"pcount {pCount}\n")
			if maxSize:
				file.write(f"maxrectres {maxSize} {maxSize}\n")
			file.write(f"bgcolor {backgroundColor[0]} {backgroundColor[1]} {backgroundColor[2]}\n")
			file.write(f"defaultcolor {defaultColor[0]} {defaultColor[1]} {defaultColor[2]}\n")
			file.write(f"userandom {usingRandom['red'].get()} {usingRandom['green'].get()} {usingRandom['blue'].get()}\n")
			# rrandom grandom brandom
			if usingRandom["red"].get():
				file.write(f"rrandom {randomColorsR[0]} {randomColorsR[1]}\n")
			if usingRandom["green"].get():
				file.write(f"grandom {randomColorsG[0]} {randomColorsG[1]}\n")
			if usingRandom["blue"].get():
				file.write(f"brandom {randomColorsB[0]} {randomColorsB[1]}\n")
			file.close()
		else:
			errorWindow = tk.Toplevel()
			for error in errors:
				tk.Label(errorWindow, text=f"Ошибка: {error}").pack()
			tk.Label(errorWindow, text="Изменения не будут внесены! Перепроверьте значения и попробуйте снова!").pack()
			errorWindow.mainloop()

	# setup controls
	if True:
		controlFrame = tk.LabelFrame(window, text="Управление")
		if not debug:
			path = os.path.abspath("./assets")
		else:
			path = os.path.abspath("./assets")
		controlOpenAssetsButton = tk.Button(controlFrame,text="Открыть папку для картинок", command=lambda: os.system(f'start "" "{path}"'))
		controlSaveConfigButton = tk.Button(controlFrame, text="Сохранить настройки", command=lambda: saveSettings())

		controlFrame.grid(row=1,column=1)
		controlOpenAssetsButton.grid(row=0,column=0)
		controlSaveConfigButton.grid(row=0,column=1)
	
		

if __name__ == '__main__':
	window = tk.Tk(screenName="Редактор конфига")
	window.title("Редактор настроек")
	window.geometry("750x405")
	window.resizable(0, 0)
	setupWindow(window)
	window.mainloop()

