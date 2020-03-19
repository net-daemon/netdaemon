DOMAIN = "netdaemon"

ATTR_CLASS = "class"
ATTR_METHOD = "method"

async def async_setup(hass, config):

    async def handle_register_service(call):
        daemon_class = call.data.get(ATTR_CLASS, "no class provided")
        daemon_method = call.data.get(ATTR_METHOD, "no method provided")

        print("Register service {}_{}".format(daemon_class, daemon_method))
        hass.services.async_register(DOMAIN, "{}_{}".format(daemon_class, daemon_method), netdaemon_noop)

    async def netdaemon_noop(call):
        # Do nothing for now, the netdaemon subscribes to this service
        pass

    # Register companion services
    hass.services.async_register(DOMAIN, "register_service", handle_register_service)
    hass.services.async_register(DOMAIN, "reload_apps", netdaemon_noop)

    # Return boolean to indicate that initialization was successful.
    return True