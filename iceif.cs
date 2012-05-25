﻿using System;
//using hms; cleaner to says hms everytime

namespace vc2ice
{
    public class IceApp 
    {
        IceGrid.QueryPrx m_query;
        IceStorm.TopicManagerPrx m_eventMgr;
        Ice.Communicator m_communicator;


        public void IceApp()
        {

            //initialize Ice
            Ice.Properties prop = Ice.Util.createProperties();
            prop.setProperty("hms.EndPoint", "tcp -p 12000 -h utopia.sintef.no");
            Ice.InitializationData iceidata = new Ice.InitializationData();
            iceidata.properties = prop;
            m_communicator = Ice.Util.initialize(iceidata); // could add sys.argv

            try
            {
                // proxy to icegrid to register our vc devices
                m_query = IceGrid.QueryPrxHelper.checkedCast(m_communicator.stringToProxy("DemoIceGrid/Query"));
                if (m_query == null)
                {
                    Console.WriteLine("invalid ICeGrid proxy");
                }
                // proxy to icestorm to publish events
                m_eventMgr = IceStorm.TopicManagerPrxHelper.checkedCast(m_communicator.propertyToProxy("EventServer/TopicManager"));
                if (m_eventMgr == null)
                {
                    Console.WriteLine("invalid IceStorm proxy");
                }
            }
            catch (Ice.NotRegisteredException)
            {
                Console.WriteLine("If we fail here it is probably because the Icebox objects are not registered");
            }
        }

        public hms.GenericEventInterfacePrx getEventPublisher(string topicName)
        {
            // Retrieve the topic
            IceStorm.TopicPrx topic;
            try
            {
                topic = m_eventMgr.retrieve(topicName);
            }
            catch (IceStorm.NoSuchTopic)
            {
                try
                {
                    topic = m_eventMgr.create(topicName);
                }
                catch (IceStorm.TopicExists)
                {
                    //maybe someone has created it inbetween so try again, otherwise raise exception
                    topic = m_eventMgr.retrieve(topicName);
                }
            }
            // Get the topic's publisher object, using towways
            Ice.ObjectPrx publisher = topic.getPublisher();         
            return hms.GenericEventInterfacePrxHelper.uncheckedCast(publisher);
        }
        public Ice.ObjectPrx[] findHolon(string type) //wraper arond findAllObjectByType for consistency with python icehms
        {
            return m_query.findAllObjectsByType(type);
        }
    }
}
