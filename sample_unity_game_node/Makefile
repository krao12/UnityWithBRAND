PROJECT=sample_unity_game_node


ROOT ?=../../../../brand-core
include $(ROOT)/setenv.mk

ifneq ($(CONDA_DEFAULT_ENV),rt)
$(error real-time conda env (rt) not active)
endif

CONDA_ROOT := $(shell conda info --root)
CONDA_PREFIX := $(CONDA_ROOT)/envs/rt

PYTHON_VERSION=3.9

PYTHON_LIB=python$(PYTHON_VERSION)

LIBPYTHON=$(CONDA_PREFIX)/lib/
INCPYTHON=$(CONDA_PREFIX)/include/$(PYTHON_LIB)

TARGET=$(PROJECT).bin
CYTHON_TARGET=$(GENERATED_PATH)/$(PROJECT).c

all:
	cp $(PROJECT).py $(PROJECT).pyx
	cython --embed $(PROJECT).pyx -o $(CYTHON_TARGET)
	gcc $(CYTHON_TARGET) -o $(TARGET) -I $(INCPYTHON) -L $(LIBPYTHON)  -Wl,-rpath=$(LIBPYTHON) -l$(PYTHON_LIB) -lpthread -lm -lutil -ldl
	$(RM) $(PROJECT).pyx
clean:
	$(RM) $(CYTHON_TARGET) $(PROJECT).pyx
