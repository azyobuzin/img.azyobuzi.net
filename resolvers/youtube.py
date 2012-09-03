# -*- coding: utf-8 -*-

import re

class youtube:
	def __str__(self):
		return "YouTube"
	
	regexStr = "^https?://(?:www\\.)?(?:youtube\\.com/watch/?\\?(?:.+&)?v=([\\w\\-]+)(?:&.*)?|youtu\\.be/([\\w\\-]+)/?(?:\\?.*)?)$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		group1 = match.group(1)
		return "http://i.ytimg.com/vi/%s/0.jpg" % (group1 if group1 is not None else match.group(2))
	
	def getLargeSize(self, match):
		group1 = match.group(1)
		return "http://i.ytimg.com/vi/%s/0.jpg" % (group1 if group1 is not None else match.group(2))
	
	def getThumbnail(self, match):
		group1 = match.group(1)
		return "http://i.ytimg.com/vi/%s/default.jpg" % (group1 if group1 is not None else match.group(2))
