# -*- coding: utf-8 -*-

import re
import MySQLdb
import httplib
import urllib2
import urlparse
from private_constant import *

class tunabe:
	def __str__(self):
		return u"つなビィ"
	
	regexStr = "^http://(?:[\\w\\-]+\\.tuna\\.be/(\\d+)\\.html|(?:www\\.)?tuna\\.be/t/([\\w\\-]+)/?)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)
		if id is None:
			id = match.group(2)
		
		if self.last is not None and self.last[0] == id:
			return self.last
		
		db = MySQLdb.connect(user=dbName, passwd=dbPassword, db=dbName, charset="utf8")
		c = db.cursor()
		
		c.execute("SELECT * FROM tunabe WHERE id = " + db.literal(id))
		sqlResult = c.fetchone()
		
		if sqlResult is None:
			httpRes = None
			
			try:
				httpRes = urllib2.urlopen("http://tuna.be/show/thumb/" + id)
			except:
				return None
			
			url = urlparse.urlparse(httpRes.geturl())
			
			httpRes.close()
			
			conn = httplib.HTTPConnection(url.netloc)
			reqPath = url.path.replace(".jpg", "_org.jpg")
			conn.request("HEAD", reqPath)
			res = conn.getresponse()
			
			org = ("http://%s%s" % (url.netloc, reqPath)) if res.status != 404 else ("http://tuna.be/show/thumb/" + id)
			
			res.close()
			
			c.execute("INSERT INTO tunabe VALUES (%s, %s)"
				% (db.literal(id), db.literal(org))
			)
			
			db.commit()
			
			self.last = [id, org]
		else:
			self.last = sqlResult
		
		return self.last
	
	def getFullSize(self, match):
		data = self.getUriData(match)
		return data[1] if data is not None else None
	
	def getLargeSize(self, match):
		return "http://tuna.be/show/thumb/" + (match.group(1) if match.group(1) is not None else match.group(2))
	
	def getThumbnail(self, match):
		return "http://tuna.be/show/thumb/" + (match.group(1) if match.group(1) is not None else match.group(2))
