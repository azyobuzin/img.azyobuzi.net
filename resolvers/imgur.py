# -*- coding: utf-8 -*-

import re

class imgur:
	def __str__(self):
		return "imgur"
	
	regexStr = "^https?://(?:www\\.|i\\.)?imgur\\.com/(\\w+)(?:\\.\\w+)?/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	#拡張子関係なく元のデータが返ってくる
	
	def getFullSize(self, match):
		return "http://i.imgur.com/%s.jpg" % match.group(1)
	
	def getLargeSize(self, match):
		return "http://i.imgur.com/%sl.jpg" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://i.imgur.com/%ss.jpg" % match.group(1)
