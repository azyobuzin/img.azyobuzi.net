# -*- coding: utf-8 -*-

import re

class mypix:
	def __str__(self):
		return "MyPix"
	
	regexStr = "^http://www\\.mypix\\.jp/app.php/picture/(\\d+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://www.mypix.jp/app.php/picture/%s/860x0.jpg" % match.group(1)
	
	def getLargeSize(self, match):
		return "http://www.mypix.jp/app.php/picture/%s/860x0.jpg" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://www.mypix.jp/app.php/picture/%s/thumb.jpg" % match.group(1)
