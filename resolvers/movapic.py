# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
from private_constant import *

class movapic:
	def __str__(self):
		return u"携帯百景"
	
	regexStr = "^http://(?:www\\.)?movapic\\.com/(?:(\\w+)/pic/(\\d+)|pic/(\w+))/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	imageRegex = re.compile("<img class=\"image\" src=\"http://image\\.movapic\\.com/pic/m_(\\w+)\\.jpeg\"/>")
	
	last = None
	
	def getUriData(self, match):
		expanded = match.group(3)
		
		if expanded is None:
			username = match.group(1)
			id = match.group(2)
			
			if self.last is not None and str(self.last[0]) == id:
				return self.last
			
			db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
			c = db.cursor()
			
			c.execute("SELECT * FROM movapic WHERE id = " + db.literal(id))
			sqlResult = c.fetchone()
			
			if sqlResult is None:
				httpRes = None
				
				httpRes = urllib2.urlopen("http://movapic.com/%s/pic/%s" % (username, id))
				
				html = httpRes.read().decode("utf-8")
				
				htmlMatch = self.imageRegex.search(html)
				
				if htmlMatch is None:
					return None
				
				expanded = htmlMatch.group(1)
				
				c.execute("INSERT INTO movapic VALUES (%s, %s)"
					% (db.literal(id), db.literal(expanded))
				)
				
				db.commit()
				
				self.last = [id, expanded]
			else:
				self.last = sqlResult
			
			return self.last
		else:
			return ["", expanded]
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		
		if data is None:
			return None
		else:
			return "http://image.movapic.com/pic/_%s.jpeg" % data[1]
	
	def getLargeSize(self, match):
		data = self.getUriData(match)
		
		if data is None:
			return None
		else:
			return "http://image.movapic.com/pic/m_%s.jpeg" % data[1]
	
	def getThumbnail(self, match):
		data = self.getUriData(match)
		
		if data is None:
			return None
		else:
			return "http://image.movapic.com/pic/t_%s.jpeg" % data[1]
